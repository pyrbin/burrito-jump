using System;

namespace pyr.Union;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public sealed class UnionAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class DefaultAttribute : Attribute
{
}
