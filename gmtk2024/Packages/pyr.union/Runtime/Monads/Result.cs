using System;

namespace pyr.Union.Monads;

[Union]
public partial record struct Result<T, E>
{
    public static partial Result<T, E> Ok(T value);
    public static partial Result<T, E> Err(E error);

    public static implicit operator Result<T, E>(T _)
    {
        return Ok(_);
    }

    public static implicit operator Result<T, E>(E _)
    {
        return Err(_);
    }


    public bool IsOk()
    {
        return Match(Ok: _ => true, Err: _ => false);
    }

    public bool IsErr()
    {
        return !IsOk();
    }

    public bool IsOk(out T value)
    {
        var (isOk, ok) = Match(
            static _ => (false, default)!,
            static value => (true, value)
        );

        value = ok;
        return isOk;
    }

    public bool IsErr(out E error)
    {
        var (isErr, err) = Match(
            static error => (true, error),
            static _ => (false, default)!
        );

        error = err;
        return isErr;
    }

    public T Or(T value)
    {
        return Match(Ok: v => v, Err: _ => value);
    }

    public T Or(Func<E, T> func)
    {
        return Match(Ok: v => v, Err: func);
    }

    public T OrThrow(string message)
    {
        return Match(Ok: v => v, Err: _ => throw new Exception(message));
    }

    public T OrThrow<F>(F err)
        where F : Exception
    {
        return Match(Ok: v => v, Err: _ => throw err);
    }

    public Result<O, E> AndThen<O>(Func<T, Result<O, E>> func)
        where O : notnull
    {
        return Match(Ok: func, Err: e => e);
    }

    public Result<O, E> Map<O>(Func<T, O> func)
        where O : notnull
    {
        return Match(Ok: v => func(v), Err: Result<O, E>.Err);
    }

    public O MapOr<O>(O or, Func<T, O> func)
        where O : notnull
    {
        return Match(Ok: func, Err: _ => or);
    }

    public O MapOrElse<O>(Func<O> or, Func<T, O> func)
        where O : notnull
    {
        return Match(Ok: func, Err: _ => or());
    }

    public Result<T, E> Where(Func<T, bool> predicate)
    {
        return Match(Ok: v => predicate(v) ? Ok(v) : Err(default!), Err: Err);
    }

    public Result<T, E> Flatten()
    {
        return Match(Ok: v => v is Result<T, E> result ? result : Ok(v), Err: Err);
    }

    public Option<T> Ok()
    {
        return Match(Ok: Option<T>.Some, Err: static _ => Option<T>.None());
    }

    public Option<E> Err()
    {
        return Match(Ok: static _ => Option<E>.None(), Err: Option<E>.Some);
    }

    public T? OkNullable()
    {
        return Match(Ok: v => (T?)v, Err: static _ => default);
    }

    public E? ErrNullable()
    {
        return Match(Ok: static _ => default, Err: v => (E?)v);
    }
}
