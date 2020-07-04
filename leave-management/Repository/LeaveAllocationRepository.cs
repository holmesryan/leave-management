using System;
using leave_management.Contracts;
using leave_management.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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


        public async Task<ICollection<LeaveAllocation>> FindAll()
        {
            return await _db.LeaveAllocations
                .Include(q => q.LeaveType)
                .ToListAsync();
        }

        public async Task<LeaveAllocation> FindById(int id)
        {
            return await _db.LeaveAllocations
                .Include(q => q.LeaveType)
                .Include(q => q.Employee)
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<bool> Exists(int id)
        {
            return await _db.LeaveRequests.AnyAsync(la => la.Id == id);
        }

        public async Task<bool> Create(LeaveAllocation entity)
        {
            await _db.LeaveAllocations.AddAsync(entity);

            return await Save();
        }

        public async Task<bool> Update(LeaveAllocation entity)
        {
            _db.LeaveAllocations.Update(entity);

            return await Save();
        }

        public async Task<bool> Delete(LeaveAllocation entity)
        {
            _db.LeaveAllocations.Remove(entity);

            return await Save();
        }

        public async Task<bool> Save()
        {
            return await _db.SaveChangesAsync() > 0;
        }

        public async Task<bool> CheckAllocation(int leaveTypeId, string employeeId)
        {
            var period = DateTime.Today.Year;
            var allocations = await FindAll();

            return allocations.Any(la => la.EmployeeId == employeeId && la.LeaveTypeId == leaveTypeId && la.Period == period);
        }

        public async Task<ICollection<LeaveAllocation>> GetLeaveAllocationsByEmployee(string id)
        {
            var period = DateTime.Today.Year;
            var allocations = await FindAll();

            return allocations.Where(la => la.EmployeeId == id && la.Period == period)
                .ToList();
        }

        public async Task<LeaveAllocation> GetLeaveAllocationsByEmployeeAndType(string employeeId, int leaveTypeId)
        {
            var period = DateTime.Today.Year;
            var allocations = await FindAll();

            return allocations.FirstOrDefault(la => la.EmployeeId == employeeId && la.Period == period && la.LeaveTypeId == leaveTypeId);
        }
    }
}
