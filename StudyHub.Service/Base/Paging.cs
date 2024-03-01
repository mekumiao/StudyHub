namespace StudyHub.Service.Base;

public record Paging {
    public readonly static Paging None = new();

    public int? Offset { get; set; }
    public int? Limit { get; set; }

    public IQueryable<T> Build<T>(IQueryable<T> queryable) where T : class {
        if (this == None) return queryable;

        if (Offset.HasValue && Offset > 0) {
            queryable = queryable.Skip(Offset.Value);
        }

        if (Limit.HasValue && Limit > 0) {
            queryable = queryable.Take(Limit.Value);
        }

        return queryable;
    }

    public static Paging FromPageNumber(int page, int limit) {
        return new Paging {
            Limit = limit,
            Offset = page > 1 ? (page - 1) * limit : 0,
        };
    }
}

public record PagingResult<T> : PagingResult where T : class {
    public T[] Items { get; set; } = [];

    public PagingResult(Paging paging) : base(paging) { }

    public PagingResult(Paging paging, int total, T[] items) : base(paging) {
        Total = total;
        Items = items;
    }
}

public record PagingResult {
    public int? Offset { get; }
    public int? Limit { get; }
    public int Total { get; set; }

    public PagingResult(Paging paging) {
        Offset = paging.Offset;
        Limit = paging.Limit;
    }
}
