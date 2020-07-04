using leave_management.Contracts;
using leave_management.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace leave_management.Repository
{
    public class LeaveRequestRepository : ILeaveRequestRepository
    {
        private readonly ApplicationDbContext _db;

        public LeaveRequestRepository(ApplicationDbContext db)
        {
            _db = db;
        }


        public async Task<ICollection<LeaveRequest>> FindAll()
        {
            var leaveRequests = _db.LeaveRequests
                .Include(lr => lr.RequestingEmployee)
                .Include(lr => lr.ApprovedBy)
                .Include(lr => lr.LeaveType)
                .ToListAsync();

            return await leaveRequests;
        }

        public async Task<LeaveRequest> FindById(int id)
        {
            var leaveRequest = await _db.LeaveRequests
                .Include(lr => lr.RequestingEmployee)
                .Include(lr => lr.ApprovedBy)
                .Include(lr => lr.LeaveType)
                .FirstOrDefaultAsync(lr => lr.Id == id);

            return leaveRequest;
        }

        public async Task<bool> Exists(int id)
        {
            return await _db.LeaveRequests.AnyAsync(lh => lh.Id == id);
        }

        public async Task<bool> Create(LeaveRequest entity)
        {
            await _db.LeaveRequests.AddAsync(entity);

            return await Save();
        }

        public async Task<bool> Update(LeaveRequest entity)
        {
            _db.LeaveRequests.Update(entity);

            return await Save();
        }

        public async Task<bool> Delete(LeaveRequest entity)
        {
            _db.LeaveRequests.Remove(entity);

            return await Save();
        }

        public async Task<bool> Save()
        {
            return await _db.SaveChangesAsync() > 0;
        }

        public async Task<ICollection<LeaveRequest>> GetLeaveRequestsByEmployee(string employeeId)
        {
            var leaveRequests = await FindAll();

            return leaveRequests.Where(lr => lr.RequestingEmployeeId == employeeId)
                .ToList();
        }
    }
}
