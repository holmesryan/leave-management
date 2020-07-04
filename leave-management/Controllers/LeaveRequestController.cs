using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using leave_management.Contracts;
using leave_management.Data;
using leave_management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace leave_management.Controllers
{
    [Authorize]
    public class LeaveRequestController : Controller
    {
        private readonly ILeaveAllocationRepository _leaveAllocRepo;
        private readonly ILeaveRequestRepository _leaveRequestRepo;
        private readonly ILeaveTypeRepository _leaveTypeRepo;
        private readonly IMapper _mapper;
        private readonly UserManager<Employee> _userManager;


        public LeaveRequestController(ILeaveAllocationRepository leaveAllocRepo, ILeaveRequestRepository leaveRequestRepo, ILeaveTypeRepository leaveTypeRepo, IMapper mapper, UserManager<Employee> userManager)
        {
            _leaveAllocRepo = leaveAllocRepo;
            _leaveRequestRepo = leaveRequestRepo;
            _leaveTypeRepo = leaveTypeRepo;
            _mapper = mapper;
            _userManager = userManager;
        }


        [Authorize(Roles = "Administrator")]
        // GET: LeaveRequest
        public async Task<ActionResult> Index()
        {
            var leaveRequests = await _leaveRequestRepo.FindAll();
            var leaveRequestsModel = _mapper.Map<List<LeaveRequestVM>>(leaveRequests);
            var model = new AdminLeaveRequestViewVM
            {
                TotalRequests = leaveRequestsModel.Count,
                ApprovedRequests = leaveRequestsModel.Count(lr => lr.Approved == true),
                PendingRequests = leaveRequestsModel.Count(lr => !lr.Approved.HasValue),
                RejectedRequests = leaveRequestsModel.Count(lr => lr.Approved == false),
                LeaveRequests = leaveRequestsModel
            };

            return View(model);
        }

        public async Task<ActionResult> MyLeave()
        {
            var employee = await _userManager.GetUserAsync(User);
            var employeeId = employee.Id;
            var employeeAllocations = await _leaveAllocRepo.GetLeaveAllocationsByEmployee(employeeId);
            var employeeRequests = await _leaveRequestRepo.GetLeaveRequestsByEmployee(employeeId);

            var employeeAllocationsModel = _mapper.Map<List<LeaveAllocationVM>>(employeeAllocations);
            var employeeRequestsModel = _mapper.Map<List<LeaveRequestVM>>(employeeRequests);

            var model = new EmployeeLeaveRequestViewVM
            {
                LeaveAllocations = employeeAllocationsModel,
                LeaveRequests = employeeRequestsModel
            };

            return View(model);
        }

        // GET: LeaveRequest/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var leaveRequest = await _leaveRequestRepo.FindById(id);
            var model = _mapper.Map<LeaveRequestVM>(leaveRequest);

            return View(model);
        }

        public async Task<ActionResult> ApproveRequest(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var leaveRequest = await _leaveRequestRepo.FindById(id);
                var allocation = await _leaveAllocRepo.GetLeaveAllocationsByEmployeeAndType(leaveRequest.RequestingEmployeeId,
                    leaveRequest.LeaveTypeId);

                leaveRequest.Approved = true;
                leaveRequest.ApprovedById = user.Id;
                leaveRequest.DateActioned = DateTime.Now;

                var success = await _leaveRequestRepo.Update(leaveRequest);

                if (!success)
                {
                    return RedirectToAction(nameof(Index));
                }

                int daysRequested = (leaveRequest.EndDate.Date - leaveRequest.StartDate.Date).Days; // doesn't account for weekends
                allocation.NumberOfDays -= daysRequested;

                await _leaveAllocRepo.Update(allocation);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<ActionResult> RejectRequest(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var leaveRequest = await _leaveRequestRepo.FindById(id);
                leaveRequest.Approved = false;
                leaveRequest.ApprovedById = user.Id;
                leaveRequest.DateActioned = DateTime.Now;

                var success = await _leaveRequestRepo.Update(leaveRequest);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: LeaveRequest/Create
        public async Task<ActionResult> Create()
        {
            var leaveTypes = await _leaveTypeRepo.FindAll();
            var leaveTypeItems = leaveTypes.Select(lt => new SelectListItem(lt.Name, lt.Id.ToString()));
            var model = new CreateLeaveRequestVM
            {
                LeaveTypes = leaveTypeItems
            };

            return View(model);
        }

        // POST: LeaveRequest/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateLeaveRequestVM model)
        {
            try
            {
                var startDate = DateTime.Parse(model.StartDate);
                var endDate = DateTime.Parse(model.EndDate);
                var leaveTypes = await _leaveTypeRepo.FindAll();
                var leaveTypeItems = leaveTypes.Select(lt => new SelectListItem(lt.Name, lt.Id.ToString()));

                model.LeaveTypes = leaveTypeItems;

                if (DateTime.Compare(startDate, endDate) > 0)
                {
                    ModelState.AddModelError("", "Start Date can not be later than End Date");
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var employee = await _userManager.GetUserAsync(User);
                var allocation = await _leaveAllocRepo.GetLeaveAllocationsByEmployeeAndType(employee.Id, model.LeaveTypeId);
                int daysRequested = (endDate.Date - startDate.Date).Days; // doesn't account for weekends

                if (daysRequested > allocation?.NumberOfDays)
                {
                    ModelState.AddModelError("", "You do not have sufficient days for this request");
                    return View(model);
                }

                var leaveRequestModel = new LeaveRequestVM
                {
                    RequestingEmployeeId = employee.Id,
                    LeaveTypeId = model.LeaveTypeId,
                    StartDate = startDate,
                    EndDate = endDate,
                    Approved = null,
                    DateRequested = DateTime.Now,
                    DateActioned = DateTime.Now // HACK - should make the property nullable
                };
                var leaveRequest = _mapper.Map<LeaveRequest>(leaveRequestModel);
                var success = await _leaveRequestRepo.Create(leaveRequest);

                if (!success)
                {
                    ModelState.AddModelError("", "Something went wrong with submitting your record");
                    return View(model);
                }

                return RedirectToAction("MyLeave");
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", "Something went wrong");
                return View(model);
            }
        }

        // GET: LeaveRequest/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: LeaveRequest/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: LeaveRequest/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: LeaveRequest/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}