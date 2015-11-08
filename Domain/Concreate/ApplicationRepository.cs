using Domain.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Models;
using Domain.Concrete;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using System.Data.Entity;
using System.Linq.Expressions;

namespace Domain.Concreate
{
    public class ApplicationRepository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _dbContext;

        public ApplicationRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> AddAsync(T t)
        {
            _dbContext.Set<T>().Add(t);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> RemoveAsync(T t)
        {
            _dbContext.Entry(t).State = EntityState.Deleted;
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> UpdateAsync(T t)
        {
            _dbContext.Entry(t).State = EntityState.Modified;
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> CountAsync()
        {
            return await _dbContext.Set<T>().CountAsync();
        }

        public async Task<List<T>> GetAllAsync()
        {
            return await _dbContext.Set<T>().ToListAsync();
        }

        public async Task<T> FindAsync(Expression<Func<T, bool>> match)
        {
            return await _dbContext.Set<T>().SingleOrDefaultAsync(match);
        }

        public async Task<List<T>> FindAllAsync(Expression<Func<T, bool>> match)
        {
            return await _dbContext.Set<T>().Where(match).ToListAsync();
        }

        public T Add(T t)
        {
            var res = _dbContext.Set<T>().Add(t);
            _dbContext.SaveChanges();
            return res;
        }
        public IEnumerable<T> AddRange(IEnumerable<T> entities)
        {
            var res = _dbContext.Set<T>().AddRange(entities);
            _dbContext.SaveChanges();
            return res;
        }

        public int Remove(T t)
        {
            _dbContext.Entry(t).State = EntityState.Deleted;
            return _dbContext.SaveChanges();
        }

        public int Update(T t)
        {
            _dbContext.Entry(t).State = EntityState.Modified;
            return _dbContext.SaveChanges();
        }
        public IEnumerable<T> UpdateRange(IEnumerable<T> entities)
        {
            foreach (var t in entities)
            {
                _dbContext.Entry(t).State = EntityState.Modified;
            }                        
            _dbContext.SaveChanges();
            return entities;
        }

        public int Count()
        {
            return _dbContext.Set<T>().Count();
        }

        public IQueryable<T> GetAll()
        {          
            return _dbContext.Set<T>().AsQueryable();
        }

        public T Find(Expression<Func<T, bool>> match)
        {
            return _dbContext.Set<T>().SingleOrDefault(match);
        }

        public IQueryable<T> FindAll(Expression<Func<T, bool>> match)
        {
            return _dbContext.Set<T>().Where(match);
        }
    }
}
