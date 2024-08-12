using pyr.Union.Monads;

namespace pyr.Union;

public static class Global
{
    /// <summary>
    ///     Global access to <see cref="Option{T}.None" />
    /// </summary>
    public static readonly Internal.None None = new();

    /// <summary>
    ///     Global access to <see cref="Option{T}.Some" />
    /// </summary>
    public static Option<T> Some<T>(T value)
        where T : notnull
    {
        return Option<T>.Some(value);
    }

    /// <summary>
    ///     Global access to <see cref="Result{T, E}.Ok" />
    /// </summary>
    public static Result<T, E> Ok<T, E>(T value)
        where T : notnull
    {
        return Result<T, E>.Ok(value);
    }

    /// <summary>
    ///     Global access to <see cref="Result{T, E}.Err" />
    /// </summary>
    public static Result<T, E> Err<T, E>(E error)
        where T : notnull
    {
        return Result<T, E>.Err(error);
    }
}
