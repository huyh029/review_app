using Microsoft.EntityFrameworkCore;
using reviewApi.Models;
using System.Linq.Expressions;

namespace reviewApi.Service.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly AppDbContext _context;

        public GenericRepository(AppDbContext context)
        {
            _context = context;
        }

        public void Add(T entity)                        => _context.Set<T>().Add(entity);
        public void AddRange(IEnumerable<T> entities)    => _context.Set<T>().AddRange(entities);
        public void Remove(T entity)                     => _context.Set<T>().Remove(entity);
        public void RemoveRange(IEnumerable<T> entities) => _context.Set<T>().RemoveRange(entities);
        public void Update(T entity)                     => _context.Set<T>().Update(entity);

        public IEnumerable<T> GetByIds<TId>(IEnumerable<TId> ids)
        {
            var key = _context.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties.First();
            return _context.Set<T>().Where(e => ids.Contains(EF.Property<TId>(e, key.Name))).ToList();
        }

        public virtual IEnumerable<T> Find(Expression<Func<T, bool>> expression)
            => _context.Set<T>().Where(expression);

        public virtual T FindFirst(Expression<Func<T, bool>> expression)
            => _context.Set<T>().FirstOrDefault(expression);

        public virtual IEnumerable<T> GetAll()
            => _context.Set<T>().ToList();

        public virtual IEnumerable<T> GetPaged(int skip, int take, Expression<Func<T, bool>> expression = null)
        {
            IQueryable<T> query = _context.Set<T>();
            if (expression != null) query = query.Where(expression);
            return query.Skip(skip).Take(take).ToList();
        }

        public virtual T GetById(object id)
            => _context.Set<T>().Find(id);

        public virtual T GetByIdInclude(object id, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _context.Set<T>();
            foreach (var include in includes)
                query = query.Include(include);
            var key = _context.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties.First();
            return query.FirstOrDefault(e => EF.Property<object>(e, key.Name).Equals(id));
        }

        public virtual int Count(Expression<Func<T, bool>> expression = null)
        {
            IQueryable<T> query = _context.Set<T>();
            if (expression != null) query = query.Where(expression);
            return query.Count();
        }

        public virtual IEnumerable<T> FindWithInclude(Expression<Func<T, bool>> expression,
            params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _context.Set<T>();
            foreach (var include in includes)
                query = query.Include(include);
            return query.Where(expression);
        }
    }
}
