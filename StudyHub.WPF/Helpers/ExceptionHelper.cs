namespace StudyHub.WPF.Helpers;

public class CannotCreateEntityException<TEntity> : Exception where TEntity : notnull {
    public CannotCreateEntityException() : base($"不能创建实体类型{typeof(TEntity).FullName}") { }
}

public sealed class ExceptionHelper {
    public static TEntity ThrowCannotCreateEntityException<TEntity>() where TEntity : notnull {
        throw new CannotCreateEntityException<TEntity>();
    }
}
