using System;
using leave_management.Contracts;
using leave_management.Data;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace leave_management.Repository
{
    public class LeaveAllocationRepository : ILeaveAllocationRepository
    {
        private readonly ApplicationDbContext _db;


        public LeaveAllocationRepository(ApplicationDbContext db)
        {
            _db = db;
        }


        public ICollection<LeaveAllocation> FindAll()
        {
            return _db.LeaveAllocations
                .Include(q => q.LeaveType)
                .ToList();
        }

        public LeaveAllocation FindById(int id)
        {
            return _db.LeaveAllocations
                .Include(q => q.LeaveType)
                .Include(q => q.Employee)
                .FirstOrDefault(q => q.Id == id);
        }

        public bool Exists(int id)
        {
            return _db.LeaveHistories.Any(la => la.Id == id);
        }

        public bool Create(LeaveAllocation entity)
        {
            _db.LeaveAllocations.Add(entity);

            return Save();
        }

        public bool Update(LeaveAllocation entity)
        {
            _db.LeaveAllocations.Update(entity);

            return Save();
        }

        public bool Delete(LeaveAllocation entity)
        {
            _db.LeaveAllocations.Remove(entity);

            return Save();
        }

        public bool Save()
        {
            return _db.SaveChanges() > 0;
        }

        public bool CheckAllocation(int leaveTypeId, string employeeId)
        {
            var period = DateTime.Today.Year;

            return FindAll().Any(la => la.EmployeeId == employeeId && la.LeaveTypeId == leaveTypeId && la.Period == period);
        }

        public ICollection<LeaveAllocation> GetLeaveAllocationsByEmployee(string id)
        {
            var period = DateTime.Today.Year;

            return FindAll()
                .Where(la => la.EmployeeId == id && la.Period == period)
                .ToList();
        }
    }
}
