namespace StudyHub.Service.Base;

public interface IQueryableFilter<TEntity> where TEntity : class {
    public IQueryable<TEntity> Build(IQueryable<TEntity> queryable);
}
