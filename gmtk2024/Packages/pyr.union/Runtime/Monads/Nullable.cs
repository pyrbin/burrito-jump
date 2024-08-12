using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace pyr.Union.Monads;

public static class NullableExtensions
{
    public static bool IsNull<T>([NotNullWhen(false)] this T? value)
    {
        return value is null || (typeof (UnityEngine.Object).IsAssignableFrom(typeof (T)))
            ? value as UnityEngine.Object == null
            : value == null;
    }

    public static bool IsNotNull<T>([NotNullWhen(true)] this T? value)
    {
        return !IsNull(value);
    }

    public static bool IsNotNull<T>(this T? value, [NotNullWhen(true)] out T result)
    {
        if (value.IsNotNull())
        {
            result = (T)value;
            return true;
        }

        result = default!;
        return false;
    }

    public static T OrThrow<T>(this T? value, string message)
    {
        if (value is not null) return value;
        throw new Exception(message);
    }

    public static T OrThrow<T, E>(this T? value, E err) where E : Exception
    {
        if (value is not null) return value;

        throw err;
    }

    public static O? AndThen<T, O>(this T? value, Func<T, O?> func)
    {
        return value is not null ? func(value) : default;
    }

    public static O? Map<T, O>(this T? value, Func<T, O> func)
    {
        return value is not null ? func(value) : default;
    }

    public static O MapOr<T, O>(this T? value, O or, Func<T, O> func)
    {
        return value is not null ? func(value) : or;
    }

    public static O MapOrElse<T, O>(this T? value, Func<O> or, Func<T, O> func)
    {
        return value is not null ? func(value) : or();
    }

    public static Result<T, E> OkOr<T, E>(this T? value, E err)
    {
        return value is not null ? Result<T, E>.Ok(value) : Result<T, E>.Err(err);
    }

    public static Result<T, E> OkOrElse<T, E>(this T? value, Func<E> err)
    {
        return value is not null ? Result<T, E>.Ok(value) : Result<T, E>.Err(err());
    }

    public static IEnumerable<T> ToEnumerable<T>(this T? value)
    {
        if (value is not null) yield return value;
    }

    public static Option<T> ToOption<T>(this T? option)
    {
        return option is null ? Option<T>.None() : Option<T>.Some(option);
    }

    public static T? ToNullable<T>(this T? value)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (value is null || value == null)
            return default;
        return value;
    }
}
