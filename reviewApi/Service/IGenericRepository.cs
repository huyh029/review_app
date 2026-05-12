using System.Linq.Expressions;

namespace reviewApi.Service
{
    public interface IGenericRepository<T> where T : class
    {
        void Add(T entity);
        void AddRange(IEnumerable<T> entities);
        IEnumerable<T> GetByIds<TId>(IEnumerable<TId> ids);
        IEnumerable<T> Find(Expression<Func<T, bool>> expression);
        T FindFirst(Expression<Func<T, bool>> expression);
        IEnumerable<T> GetAll();
        IEnumerable<T> GetPaged(int skip, int take, Expression<Func<T, bool>> expression = null);
        T GetById(object id);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
        void Update(T entity);
        T GetByIdInclude(object id, params Expression<Func<T, object>>[] includes);
        int Count(Expression<Func<T, bool>> expression = null);
        IEnumerable<T> FindWithInclude(Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] includes);
    }
}
