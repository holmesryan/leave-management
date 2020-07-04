using leave_management.Contracts;
using leave_management.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace leave_management.Repository
{
    public class LeaveTypeRepository : ILeaveTypeRepository
    {
        private readonly ApplicationDbContext _db;


        public LeaveTypeRepository(ApplicationDbContext db)
        {
            _db = db;
        }


        public async Task<ICollection<LeaveType>> FindAll()
        {
            return await _db.LeaveTypes.ToListAsync();
        }

        public async Task<LeaveType> FindById(int id)
        {
            // equivalent to _db.LeaveTypes.FirstOrDefault(lt => lt.Id == id)
            return await _db.LeaveTypes.FindAsync(id);
        }

        public async Task<bool> Exists(int id)
        {
            return await _db.LeaveTypes.AnyAsync(lt => lt.Id == id);
        }

        public async Task<bool> Create(LeaveType entity)
        {
            await _db.LeaveTypes.AddAsync(entity);

            return await Save();
        }

        public async Task<bool> Update(LeaveType entity)
        {
            _db.LeaveTypes.Update(entity);

            return await Save();
        }

        public async Task<bool> Delete(LeaveType entity)
        {
            _db.LeaveTypes.Remove(entity);

            return await Save();
        }

        public async Task<bool> Save()
        {
            // SaveChanges returns # of records modified
            return await _db.SaveChangesAsync() > 0;
        }

        public ICollection<LeaveType> GetEmployeesByLeaveType(int id)
        {
            throw new NotImplementedException();
        }
    }
}
