using leave_management.Contracts;
using leave_management.Data;
using System.Collections.Generic;
using System.Linq;
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


        public ICollection<LeaveRequest> FindAll()
        {
            var leaveRequests = _db.LeaveRequests
                .Include(lr => lr.RequestingEmployee)
                .Include(lr => lr.ApprovedBy)
                .Include(lr => lr.LeaveType)
                .ToList();

            return leaveRequests;
        }

        public LeaveRequest FindById(int id)
        {
            var leaveRequest = _db.LeaveRequests
                .Include(lr => lr.RequestingEmployee)
                .Include(lr => lr.ApprovedBy)
                .Include(lr => lr.LeaveType)
                .FirstOrDefault(lr => lr.Id == id);

            return leaveRequest;
        }

        public bool Exists(int id)
        {
            return _db.LeaveRequests.Any(lh => lh.Id == id);
        }

        public bool Create(LeaveRequest entity)
        {
            _db.LeaveRequests.Add(entity);

            return Save();
        }

        public bool Update(LeaveRequest entity)
        {
            _db.LeaveRequests.Update(entity);

            return Save();
        }

        public bool Delete(LeaveRequest entity)
        {
            _db.LeaveRequests.Remove(entity);

            return Save();
        }

        public bool Save()
        {
            return _db.SaveChanges() > 0;
        }

        public ICollection<LeaveRequest> GetLeaveRequestsByEmployee(string employeeId)
        {
            return FindAll().Where(lr => lr.RequestingEmployeeId == employeeId)
                .ToList();
        }
    }
}
