using AutoMapper;
using leave_management.Contracts;
using leave_management.Data;
using leave_management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace leave_management.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class LeaveTypesController : Controller
    {
        private readonly ILeaveTypeRepository _repo;
        private readonly IMapper _mapper;

        public LeaveTypesController(ILeaveTypeRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        // GET: LeaveTypes
        public async Task<ActionResult> Index()
        {
            var leaveTypes = await _repo.FindAll();
            var model = _mapper.Map<ICollection<LeaveType>, List<LeaveTypeVM>>(leaveTypes);

            return View(model);
        }

        // GET: LeaveTypes/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var exists = await _repo.Exists(id); // is awaiting here any different than if (!_repo.Exists(id).Result) ?
            if (!exists)
            {
                return NotFound();
            }

            var leaveType = await _repo.FindById(id);
            var model = _mapper.Map<LeaveTypeVM>(leaveType);

            return View(model);
        }

        // GET: LeaveTypes/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: LeaveTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(LeaveTypeVM model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var leaveType = _mapper.Map<LeaveType>(model);
                leaveType.DateCreated = DateTime.Now;

                var success = await _repo.Create(leaveType);

                if (!success)
                {
                    ModelState.AddModelError("", "Something went wrong...");
                    return View(model);
                }

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ModelState.AddModelError("", "Something went wrong...");
                return View(model);
            }
        }

        // GET: LeaveTypes/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            if (!_repo.Exists(id).Result)
            {
                return NotFound();
            }

            var leaveType = _repo.FindById(id);
            var model = _mapper.Map<LeaveTypeVM>(leaveType);

            return View(model);
        }

        // POST: LeaveTypes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(LeaveTypeVM model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var leaveType = _mapper.Map<LeaveType>(model);

                var success = await _repo.Update(leaveType);
                if (!success)
                {
                    ModelState.AddModelError("", "Something went wrong...");
                    return View(model);
                }

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ModelState.AddModelError("", "Something went wrong...");
                return View(model);
            }
        }

        // GET: LeaveTypes/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            //if (!_repo.Exists(id))
            //{
            //    return NotFound();
            //}

            //var leaveType = _repo.FindById(id);
            //var model = _mapper.Map<LeaveTypeVM>(leaveType);

            //return View(model);

            // copied from POST method even though it's a terrible idea to delete from a GET
            // this was done in the course
            var leaveType = await _repo.FindById(id);
            if (leaveType is null)
            {
                return NotFound();
            }

            var success = await _repo.Delete(leaveType);
            if (!success)
            {
                return BadRequest();
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: LeaveTypes/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id, LeaveTypeVM model)
        {
            try
            {
                var leaveType = await _repo.FindById(id);
                if (leaveType is null)
                {
                    return NotFound();
                }

                var success = await _repo.Delete(leaveType);
                if (!success)
                {
                    return View(model);
                }
                
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View(model);
            }
        }
    }
}