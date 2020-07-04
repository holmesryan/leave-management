using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using leave_management.Contracts;
using leave_management.Data;
using leave_management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace leave_management.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class LeaveAllocationController : Controller
    {
        private readonly ILeaveTypeRepository _leaveTypeRepo;
        private readonly ILeaveAllocationRepository _leaveAllocationRepo;
        private readonly IMapper _mapper;
        private readonly UserManager<Employee> _userManager;

        public LeaveAllocationController(ILeaveTypeRepository leaveTypeRepo, ILeaveAllocationRepository leaveAllocationRepo, IMapper mapper, UserManager<Employee> userManager)
        {
            _leaveTypeRepo = leaveTypeRepo;
            _leaveAllocationRepo = leaveAllocationRepo;
            _mapper = mapper;
            _userManager = userManager;
        }

        // GET: LeaveAllocation
        public async Task<ActionResult> Index()
        {
            var leaveTypes = await _leaveTypeRepo.FindAll();
            var mappedLeaveTypes = _mapper.Map<ICollection<LeaveType>, List<LeaveTypeVM>>(leaveTypes);
            var model = new CreateLeaveAllocationVM
            {
                LeaveTypes = mappedLeaveTypes,
                NumberUpdated = 0
            };

            return View(model);
        }

        public async Task<ActionResult> SetLeave(int id)
        {
            var leaveType = await _leaveTypeRepo.FindById(id);
            var employees = await _userManager.GetUsersInRoleAsync("Employee");

            foreach (var emp in employees)
            {
                if (await _leaveAllocationRepo.CheckAllocation(id, emp.Id))
                    continue;

                var allocation = new LeaveAllocationVM
                {
                    DateCreated = DateTime.Now,
                    EmployeeId = emp.Id,
                    LeaveTypeId = id,
                    NumberOfDays = leaveType.DefaultDays,
                    Period = DateTime.Today.Year
                };

                var leaveAllocation = _mapper.Map<LeaveAllocation>(allocation);
                await _leaveAllocationRepo.Create(leaveAllocation);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<ActionResult> ListEmployees()
        {
            var employees = await _userManager.GetUsersInRoleAsync("Employee");
            var model = _mapper.Map<List<EmployeeVM>>(employees);

            return View(model);
        }

        // GET: LeaveAllocation/Details/5
        public async Task<ActionResult> Details(string id)
        {
            var employee = _mapper.Map<EmployeeVM>(await _userManager.FindByIdAsync(id));
            var allocations = _mapper.Map<List<LeaveAllocationVM>>(await _leaveAllocationRepo.GetLeaveAllocationsByEmployee(id));
            var model = new ViewAllocationsVM
            {
                Employee = employee,
                LeaveAllocations = allocations
            };

            return View(model);
        }

        // GET: LeaveAllocation/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: LeaveAllocation/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(IFormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: LeaveAllocation/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var leaveAllocation = await _leaveAllocationRepo.FindById(id);
            var model = _mapper.Map<EditLeaveAllocationVM>(leaveAllocation);

            return View(model);
        }

        // POST: LeaveAllocation/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(EditLeaveAllocationVM model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                var record = await _leaveAllocationRepo.FindById(model.Id);
                record.NumberOfDays = model.NumberOfDays;
                //var allocation = _mapper.Map<LeaveAllocation>(model);
                //var success = _leaveAllocationRepo.Update(allocation);
                var success = await _leaveAllocationRepo.Update(record);

                if (!success)
                {
                    ModelState.AddModelError("", "Error while saving");
                    return View(model);
                }

                return RedirectToAction(nameof(Details), new {id = model.EmployeeId});
            }
            catch
            {
                return View(model);
            }
        }

        // GET: LeaveAllocation/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: LeaveAllocation/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
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