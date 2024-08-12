using System;
using System.Collections.Generic;
using pyr.Union.Internal;

namespace pyr.Union.Monads;

[Union]
public partial record struct Option<T>
{
    [Default]
    public static partial Option<T> None();

    public static partial Option<T> Some(T value);

    public static implicit operator Option<T>(None _)
    {
        return None();
    }

    public static implicit operator Option<T>(T? _)
    {
        return _.IsNull() ? None() : Some(_!);
    }

    public bool IsNone()
    {
        return !IsSome();
    }

    public bool IsSome()
    {
        return MatchOr(Some: _ => true, _: () => false);
    }

    public bool IsSome(out T value)
    {
        var (isSome, some) = Match(
            static () => (false, default)!,
            static value => (true, value)
        );

        value = some;
        return isSome;
    }

    public bool IsSomeAnd(Func<T, bool> func)
    {
        return Match(Some: func, None: () => false);
    }

    public T Or(T value)
    {
        return Match(Some: v => v, None: () => value);
    }

    public T Or(Func<T> func)
    {
        return Match(Some: v => v, None: func);
    }

    public T OrDefault()
    {
        return Match(Some: v => v, None: () => default!);
    }

    public T OrThrow(string message)
    {
        return Match(Some: v => v, None: () => throw new Exception(message));
    }

    public T OrThrow<E>(E err)
        where E : Exception
    {
        return Match(Some: v => v, None: () => throw err);
    }

    public Option<O> And<O>(Option<O> option)
    {
        return Match(Some: _ => option, None: Option<O>.None);
    }

    public Option<O> AndThen<O>(Func<T, Option<O>> func)
        where O : notnull
    {
        return Match(Some: func, None: Option<O>.None);
    }

    public Option<O> Map<O>(Func<T, O> func)
        where O : notnull
    {
        return Match(Some: v => func(v), None: Option<O>.None);
    }

    public O MapOr<O>(O or, Func<T, O> func)
        where O : notnull
    {
        return Match(Some: func, None: () => or);
    }

    public O MapOrElse<O>(Func<O> or, Func<T, O> func)
    {
        return Match(Some: func, None: or);
    }

    public Option<T> Where(Func<T, bool> predicate)
    {
        return Match(Some: v => predicate(v) ? Some(v) : None(), None: None);
    }

    public Option<T> Flatten()
    {
        return Match(Some: v => v is Option<T> option ? option : Some(v), None: None);
    }

    public Result<T, E> OkOr<E>(E err)
    {
        return Match(Some: Result<T, E>.Ok, None: () => Result<T, E>.Err(err));
    }

    public Result<T, E> OkOrElse<E>(Func<E> err)
    {
        return Match(Some: Result<T, E>.Ok, None: () => Result<T, E>.Err(err()));
    }

    public IEnumerable<T> AsEnumerable()
    {
        if (IsSome(out var value)) yield return value;
    }

    public T? ToNullable()
    {
        return Match(Some: v => (T?)v, None: () => default);
    }
}
