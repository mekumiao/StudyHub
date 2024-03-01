namespace StudyHub.Service.Base;

public interface ICurdService<TEntity, TKey, TGetOutputDto, TGetListOutputDto, TGetListFilter, TCreateInput, TUpdateInput>
    where TEntity : class
    where TKey : notnull
    where TGetOutputDto : class
    where TGetListOutputDto : class
    where TGetListFilter : class, IQueryableFilter<TEntity>
    where TCreateInput : class
    where TUpdateInput : class {
    public Task<ServiceResult<TGetOutputDto>> GetEntityByIdAsync(TKey id);
    public Task<ServiceResult<PagingResult<TGetListOutputDto>>> GetListAsync(TGetListFilter filter, Paging paging);
    public Task<ServiceResult<TGetOutputDto>> CreateAsync(TCreateInput input);
    public Task<ServiceResult<TGetOutputDto>> UpdateAsync(TKey id, TUpdateInput dto);
    public Task<ServiceResult> DeleteAsync(TKey id);
    public Task<ServiceResult> DeleteItemsAsync(TKey[] ids);
}
