using CrossPlanner.Domain.Context;
using CrossPlanner.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CrossPlanner.Repository.Repository
{
    public class RepositoryBase<T> : IRepositoryBase<T> where T : class
    {
        protected ApplicationContext _applicationContext {  get; set; }
        protected DbSet<T> _dbSet;

        public RepositoryBase(ApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
            _dbSet = _applicationContext.Set<T>();
        }

        public void Create(T entity)
        {
            _applicationContext.Set<T>().Add(entity);
        }

        public void Delete(T entity)
        {
            _applicationContext.Set<T>().Remove(entity);
        }

        public IQueryable<T> FindAll()
        {
            return _applicationContext.Set<T>().AsNoTracking();
        }

        public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression)
        {
            return _applicationContext.Set<T>().Where(expression).AsNoTracking();
        }

        public void Update(T entity)
        {
            _applicationContext.Set<T>().Update(entity);
        }
    }
}
