using MapsterMapper;

using Microsoft.EntityFrameworkCore;

using StudyHub.Storage.DbContexts;

namespace StudyHub.Service.Base;

public abstract class CurdService<TEntity, TKey, TGetOutputDto, TGetListOutputDto, TGetListFilter, TCreateInput, TUpdateInput>(
    StudyHubDbContext dbContext,
    IMapper mapper) : ICurdService<TEntity, TKey, TGetOutputDto, TGetListOutputDto, TGetListFilter, TCreateInput, TUpdateInput>
    where TEntity : class
    where TKey : notnull
    where TGetOutputDto : class
    where TGetListOutputDto : class
    where TGetListFilter : class, IQueryableFilter<TEntity>
    where TCreateInput : class
    where TUpdateInput : class {
    protected readonly StudyHubDbContext _dbContext = dbContext;
    protected readonly IMapper _mapper = mapper;

    public async virtual Task<ServiceResult<TGetOutputDto>> GetEntityByIdAsync(TKey id) {
        var item = await _dbContext.Set<TEntity>().FindAsync(id);
        if (item == null) {
            return ServiceResult.NotFound<TGetOutputDto>();
        }
        var result = _mapper.Map<TGetOutputDto>(item);
        return ServiceResult.Ok(result);
    }

    public async virtual Task<ServiceResult<PagingResult<TGetListOutputDto>>> GetListAsync(TGetListFilter filter, Paging paging) {
        var queryable = QueryAll();

        queryable = filter.Build(queryable);
        var total = await queryable.CountAsync();

        queryable = paging.Build(queryable);
        var result = await queryable.ToListAsync();

        var resultItems = _mapper.Map<TGetListOutputDto[]>(result);
        var pageResult = new PagingResult<TGetListOutputDto>(paging, total, resultItems);
        return ServiceResult.Ok(pageResult);
    }

    public async virtual Task<ServiceResult<TGetOutputDto>> CreateAsync(TCreateInput dto) {
        var item = _mapper.Map<TEntity>(dto);
        _dbContext.Add(item);
        await _dbContext.SaveChangesAsync();
        return ServiceResult.Ok(_mapper.Map<TGetOutputDto>(item));
    }

    public async virtual Task<ServiceResult<TGetOutputDto>> UpdateAsync(TKey id, TUpdateInput dto) {
        var item = await _dbContext.Set<TEntity>().FindAsync(id);
        if (item is null) {
            return ServiceResult.NotFound<TGetOutputDto>();
        }
        _mapper.Map(dto, item);
        try {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException) {
            return ServiceResult.NotFound<TGetOutputDto>();
        }
        return ServiceResult.Ok(_mapper.Map<TGetOutputDto>(item));
    }

    public async virtual Task<ServiceResult> DeleteAsync(TKey id) {
        var item = await _dbContext.Set<TEntity>().FindAsync(id);
        if (item is null) {
            return ServiceResult.NotFound();
        }
        _dbContext.Set<TEntity>().Remove(item);
        try {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException) {
        }
        return ServiceResult.Ok();
    }

    protected virtual IQueryable<TEntity> QueryAll() {
        return _dbContext.Set<TEntity>().AsNoTracking();
    }

    public abstract Task<ServiceResult> DeleteItemsAsync(TKey[] ids);
}
