using leave_management.Contracts;
using leave_management.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace leave_management.Repository
{
    public class LeaveTypeRepository : ILeaveTypeRepository
    {
        private readonly ApplicationDbContext _db;


        public LeaveTypeRepository(ApplicationDbContext db)
        {
            _db = db;
        }


        public ICollection<LeaveType> FindAll()
        {
            return _db.LeaveTypes.ToList();
        }

        public LeaveType FindById(int id)
        {
            // equivalent to _db.LeaveTypes.FirstOrDefault(lt => lt.Id == id)
            return _db.LeaveTypes.Find(id);
        }

        public bool Create(LeaveType entity)
        {
            _db.LeaveTypes.Add(entity);

            return Save();
        }

        public bool Update(LeaveType entity)
        {
            _db.LeaveTypes.Update(entity);

            return Save();
        }

        public bool Delete(LeaveType entity)
        {
            _db.LeaveTypes.Remove(entity);

            return Save();
        }

        public bool Save()
        {
            // SaveChanges returns # of records modified
            return _db.SaveChanges() > 0;
        }

        public ICollection<LeaveType> GetEmployeesByLeaveType(int id)
        {
            throw new NotImplementedException();
        }
    }
}
