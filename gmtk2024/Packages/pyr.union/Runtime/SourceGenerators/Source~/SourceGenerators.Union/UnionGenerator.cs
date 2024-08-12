using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using pyr.Union.SourceGenerators.Common;

namespace pyr.Union.SourceGenerators;

[Generator]
public class UnionGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new UnionTypeReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var receiver = context.SyntaxReceiver as UnionTypeReceiver ??
                       throw new InvalidCastException("Generator did not initialize properly");

        if (!UnionGeneratorInternal.TryGetInstance(context.Compilation, out var generator)) return;
        foreach (var candidate in receiver.Candidates)
        {
            if (generator == null || !generator.Execute(candidate, out var hintName, out var source)) continue;
            if (hintName != null) context.AddSource(hintName, source!);
        }
    }
}

internal class UnionGeneratorInternal
{
    private const string InternalKindName = "@Internal_Kind";
    private const string InternalValueName = "@Internal_Value";

    private readonly Compilation _compilation;
    private readonly INamedTypeSymbol? _unionAttributeSymbol;

    private UnionGeneratorInternal(Compilation compilation, INamedTypeSymbol symbol)
    {
        _compilation = compilation;
        _unionAttributeSymbol = symbol;
    }

    public static bool TryGetInstance(Compilation compilation, out UnionGeneratorInternal? generator)
    {
        const string unionAttributeMetadataName = "pyr.Union.UnionAttribute";

        var symbol = compilation.GetTypeByMetadataName(unionAttributeMetadataName);

        if (symbol is null)
        {
            generator = default;
            return false;
        }

        generator = new UnionGeneratorInternal(compilation, symbol);
        return true;
    }

    internal bool Execute(TypeDeclarationSyntax typeDeclaration, out string? hintName, out SourceText? source)
    {
        var model = _compilation.GetSemanticModel(typeDeclaration.SyntaxTree);

        if (IsUnionType(model, typeDeclaration, out var typeSymbol))
        {
            hintName = GetHintName(typeSymbol!);
            source = GenerateSource(model, typeSymbol!);
            return true;
        }

        hintName = default;
        source = default;
        return false;
    }

    private static string GetHintName(INamedTypeSymbol typeSymbol)
    {
        return typeSymbol.MetadataName + ".cs";
    }

    private bool IsUnionType(SemanticModel model, TypeDeclarationSyntax typeDeclaration,
        out INamedTypeSymbol? declarationSymbol)
    {
        declarationSymbol = model.GetDeclaredSymbol(typeDeclaration) as INamedTypeSymbol;
        if (declarationSymbol is null)
            return false;

        if (declarationSymbol.IsStatic || declarationSymbol.IsAnonymousType || declarationSymbol.IsTupleType)
            return false;

        return declarationSymbol
            .GetAttributes()
            .Any(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, _unionAttributeSymbol));
    }

    private SourceText GenerateSource(SemanticModel model, INamedTypeSymbol type)
    {
        var str = IndentStringBuilder.Space();
        var union = CreateUnion(type);
        var variants = union.Variants;

        GenerateUsings(str);
        GenerateNamespace(str, type);
        GenerateUnionTypeHeader(str, type);
        GenerateUnionBody(str, type, union, variants);

        return SourceText.From(str.ToString().NormalizeLineEndings(), Encoding.UTF8);
    }

    private static void GenerateUsings(IndentStringBuilder str)
    {
        var usings = new[]
        {
            "System",
            "System.Collections.Generic",
            "System.Runtime.CompilerServices",
            "System.Runtime.InteropServices",
            "System.Linq",
            "UnityEngine"
        };

        foreach (var @using in usings.OrderBy(u => (u.StartsWith("System"), u)).Select(u => $"using {u};"))
        {
            str.AppendLine(@using);
        }

        str.AppendLine();
    }

    private static void GenerateNamespace(IndentStringBuilder str, INamedTypeSymbol type)
    {
        var types = type.Unfold(x => x.ContainingType).Reverse().Append(type).ToArray();
        var declaredNamespace = types.First().ContainingNamespace.GetFullName();
        str.AppendLine("#nullable enable");
        str.AppendLine();
        str.AppendLine($"namespace {declaredNamespace};");
        str.AppendLine();
    }

    private static void GenerateUnionTypeHeader(IndentStringBuilder str, INamedTypeSymbol type)
    {
        var accessibility = type.DeclaredAccessibility.GetSourceText();
        var typeKeywords = type switch
        {
            { IsRecord: true, IsValueType: true, IsReadOnly: true, IsRefLikeType: true } =>
                "readonly ref partial record struct",
            { IsRecord: true, IsValueType: true, IsReadOnly: true } => "readonly partial record struct",
            { IsRecord: true, IsValueType: true } => "partial record struct",
            { IsRecord: true } => "partial record",
            { IsValueType: true, IsReadOnly: true, IsRefLikeType: true } =>
                "readonly ref partial struct",
            { IsValueType: true, IsReadOnly: true } => "readonly partial struct",
            { IsValueType: true } => "partial struct",
            _ => "partial class"
        };

        var typeName = type.GetLocalName();
        str.AppendLine("[System.Serializable]");
        str.AppendLine($"{accessibility} {typeKeywords} {typeName}");
        str.AppendLine("{");
        str.Indent();
    }

    private static void GenerateUnionBody(IndentStringBuilder str, INamedTypeSymbol type, Union union,
        UnionVariant[] variants)
    {
        GenerateVariantConstructors(str, type, union, variants);
        GenerateInternalKind(str, variants);
        GenerateInternalValue(str, type, union, variants);
        GenerateKindAndValueProperties(str, type);
        GeneratePrivateConstructor(str, type);
        GenerateMatchMethods(str, union, variants);
        GenerateDoMethod(str, union, variants);
        GenerateBoxedValueMethod(str, union, variants);
        GenerateToStringMethod(str, union, variants);

        // editor
        var indent = str.IndentDepth;
        str.ClearIndent();
        str.AppendLine("#if UNITY_EDITOR");
        str.AppendLine();
        str.SetIndent(indent);
        GenerateEditorGetPropertyNamesMethod(str, union, variants);
        str.ClearIndent();
        str.AppendLine("#endif");
        str.SetIndent(indent);

        str.Unindent();
        str.AppendLine("}");
    }

    private static void GenerateVariantConstructors(IndentStringBuilder str, INamedTypeSymbol type, Union union,
        UnionVariant[] variants)
    {
        var typeName = type.GetLocalName();

        foreach (var variant in variants)
        {
            var parameters = variant.Parameters
                .Select(param => $"{param.TypeName} {param.CamelName.EscapeIfKeyword()}")
                .Join(", ");

            var signature = $"{variant.Accessibility} static partial {typeName} {variant.PascalName}({parameters}) => ";
            var valueIdents = variant.Parameters
                .Select(p => $"{union.ValuePropertyName(p.TypeNameWithIndex)} = {p.CamelName.EscapeIfKeyword()}")
                .Join(", ");

            var value = variant switch
            {
                { EmptyVariant: true } => "default",
                _ => $"new () {{ {valueIdents} }}"
            };

            var body = $"new ({variant.KindProperty}, {value})";

            str.AppendLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
            str.AppendLine($"{signature}{body};");
            str.AppendLine();
        }
    }

    private static void GenerateInternalKind(IndentStringBuilder str, UnionVariant[] variants)
    {
        str.AppendLine("[System.Serializable]");
        str.AppendLine("private enum @Internal_Kind : byte");
        str.AppendLine("{");
        str.Indent();

        foreach (var variant in variants)
        {
            str.Append($"{variant.PascalName}");
            str.Append(variant.Index == 0
                ? $" = 0x00{(variant == variants.Last() ? "" : ",")}"
                : variant == variants.Last()
                    ? ""
                    : ",");
            str.AppendLine("");
        }

        str.Unindent();
        str.AppendLine("}");
        str.AppendLine("");
    }

    private static void GenerateInternalValue(IndentStringBuilder str, INamedTypeSymbol type, Union union,
        UnionVariant[] variants)
    {
        str.AppendLine("[System.Serializable]");
        str.AppendLine("[StructLayout(LayoutKind.Sequential)]");
        str.AppendLine("private record struct @Internal_Value");
        str.AppendLine("(");
        str.Indent();

        var length = union.ValueIdenfifiers.Count;
        var i = 0;
        foreach (var (_, value) in union.ValueIdenfifiers)
        {
            i++;
            str.AppendLine($"[field: SerializeField] {value.TypeName} {value.TypePropertyName}{(i == length ? "" : ",")}");
        }

        str.Unindent();
        str.AppendLine(");");
        str.AppendLine();
    }

    private static void GenerateKindAndValueProperties(IndentStringBuilder str, INamedTypeSymbol type)
    {
        var autoSetter = type.IsReadOnly ? "init" : "set";
        str.AppendLine("[field: SerializeField]");
        str.AppendLine($"private {InternalKindName} __Kind {{ get; {autoSetter}; }}");
        str.AppendLine();
        str.AppendLine("[field: SerializeField]");
        str.AppendLine($"private {InternalValueName} __Value {{ get; {autoSetter}; }}");
        str.AppendLine();
    }

    private static void GeneratePrivateConstructor(IndentStringBuilder str, INamedTypeSymbol type)
    {
        str.AppendLine($"private {type.Name}({InternalKindName} kind, {InternalValueName} value)");
        str.AppendLine("{");
        str.Indent();
        str.AppendLine("__Kind = kind;");
        str.AppendLine("__Value = value;");
        str.Unindent();
        str.AppendLine("}");
        str.AppendLine();
    }

    private static void GenerateMatchMethods(IndentStringBuilder str, Union union, UnionVariant[] variants)
    {
        GenerateMatchMethod(str, union, variants, false);
        str.AppendLine("#pragma warning disable");
        str.AppendLine();
        GenerateMatchMethod(str, union, variants, true);
        str.AppendLine("#pragma warning restore");
        str.AppendLine();
    }

    private static void GenerateMatchMethod(IndentStringBuilder str, Union union, UnionVariant[] variants, bool withDefault)
    {
        var suffix = withDefault ? "?" : "";
        var nullable = withDefault ? "= null" : "";
        var name = withDefault ? "MatchOr" : "Match";

        str.AppendLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
        str.Append($"public O {name}<O>(");
        var parametersList = variants.Select(
            v =>
                $"Func<{v.Parameters.Select(param => param.TypeName)
                    .Append("O")
                    .Join(",")}>{suffix} {v.PascalName.EscapeIfKeyword()}{nullable}"
        );

        if (withDefault)
            parametersList = parametersList
                .Prepend("Func<O> _");

        var parameters = parametersList.Join(", ");

        str.Append(parameters);
        str.AppendLine(") =>");
        str.Indent();

        str.AppendLine("__Kind switch");
        str.AppendLine("{");

        str.Indent();

        foreach (var v in variants)
        {
            var condition = withDefault ? $" when {v.PascalName.EscapeIfKeyword()} is not null" : "";
            var arguments = v.Parameters
                .Select(param => $"({param.TypeName})__Value.{union.ValuePropertyName(param.TypeNameWithIndex)}!").Join(",");
            var comma = withDefault ? "," : v == variants.Last() ? "" : ",";

            str.AppendLine($"{v.KindProperty}{condition} => {v.PascalName.EscapeIfKeyword()}({arguments}){comma}");
        }

        if (withDefault) str.AppendLine("_ => _.Invoke()");

        str.Unindent();
        str.AppendLine("};");
        str.Unindent();
        str.AppendLine("");
    }

    private static void GenerateDoMethod(IndentStringBuilder str, Union union, UnionVariant[] variants)
    {
        str.AppendLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
        str.Append("public void Do(");

        var parameters = variants.Select(
                v =>
                    $"Action<{v.Parameters.Select(param => param.TypeName).Join(",")}>? {v.PascalName.EscapeIfKeyword()} = null"
            )
            .Select(x => x.Replace("Action<>?", "Action?"))
            .Append("Action? _ = null")
            .Join(", ");

        str.Append(parameters);
        str.AppendLine(")");
        str.AppendLine("{");
        str.Indent();
        str.AppendLine("switch (__Kind)");
        str.AppendLine("{");
        str.Indent();
        foreach (var v in variants)
        {
            var arguments = v.Parameters
                .Select(param => $"({param.TypeName})__Value.{union.ValuePropertyName(param.TypeNameWithIndex)}!").Join(",");
            str.AppendLine(
                $"case {v.KindProperty} when {v.PascalName.EscapeIfKeyword()} is not null: {v.PascalName.EscapeIfKeyword()}({arguments}); break;");
        }

        str.AppendLine("default: _?.Invoke(); break;");
        str.Unindent();
        str.AppendLine("}");
        str.Unindent();
        str.AppendLine("}");
        str.AppendLine();
    }

    private static void GenerateBoxedValueMethod(IndentStringBuilder str, Union union, UnionVariant[] variants)
    {
        str.AppendLine("public object Boxed => __Kind switch");
        str.AppendLine("{");
        str.Indent();

        foreach (var v in variants)
        {
            var comma = v == variants.Last() ? "" : ",";

            if (v.Parameters.Length == 0)
            {
                str.AppendLine($"{v.KindProperty} => default(ValueType){comma}");
                continue;
            }

            var arguments = v.Parameters
                .Select(param => $"({param.TypeName})__Value.{union.ValuePropertyName(param.TypeNameWithIndex)}!")
                .ToArray();
            var argumentsString = arguments.Join(",");
            argumentsString = $"({argumentsString})";

            str.AppendLine($"{v.KindProperty} => {argumentsString}{comma}");
        }
        str.Unindent();
        str.AppendLine("};");
        str.AppendLine();
    }

    private static void GenerateToStringMethod(IndentStringBuilder str, Union union, UnionVariant[] variants)
    {
        str.AppendLine("public override string ToString()");
        str.AppendLine("{");
        str.Indent();
        str.AppendLine("switch (__Kind)");
        str.AppendLine("{");
        str.Indent();
        foreach (var v in variants)
        {
            str.AppendLine($"case {v.KindProperty}:");
            str.Indent();
            if (v.Parameters.Length == 0)
            {
                str.AppendLine($"return $\"{{__Kind.ToString()}}\";");
            }
            else
            {
                var names = $"k_{v.PascalName}Names";
                str.AppendLine($"string[] {names} = {{{v.Parameters.Select(x => "\"" + x.PascalName + "\"").Join(",")}}};");
                str.AppendLine($"return $\"{{__Kind.ToString()}}({{string.Join(\", \", this.Boxed.ToString().Trim('(', ')').Split(',').Select((v,i) => $\"{{{names}[i]}}: {{v}}\"))}})\";");
            }
            str.Unindent();
        }
        str.AppendLine("default: return $\"{{__Kind.ToString()}}\";");
        str.Unindent();
        str.AppendLine("}");
        str.Unindent();
        str.AppendLine("}");
        str.AppendLine();
    }

    private static void GenerateEditorGetPropertyNamesMethod(IndentStringBuilder str, Union union, UnionVariant[] variants)
    {
        str.AppendLine("public IEnumerable<(string, string)> GetValuePropertyNames(int index)");
        str.AppendLine("{");
        str.Indent();
        str.AppendLine(
            $"var kindValues = ({InternalKindName}[])Enum.GetValues(typeof({InternalKindName}));");
        str.AppendLine("var kind = kindValues[index];");
        str.AppendLine("switch (kind)");
        str.AppendLine("{");
        str.Indent();
        foreach (var v in variants)
        {
            str.AppendLine($"case {v.KindProperty}:");
            str.Indent();
            if (v.Parameters.Length == 0)
            {
                str.AppendLine($"return ArraySegment<(string, string)>.Empty;");
            }
            else
            {
                var names = $"k_{v.PascalName}Names";
                str.AppendLine($"(string, string)[] {names} = {{{v.Parameters.Select(x => $"(\"{x.PascalName}\", \"{union.ValuePropertyName(x.TypeNameWithIndex)}\")").Join(",")}}};");
                str.AppendLine($"return {names}.AsEnumerable();");
            }
            str.Unindent();
        }
        str.AppendLine("default: return ArraySegment<(string, string)>.Empty;");
        str.Unindent();
        str.AppendLine("}");
        str.Unindent();
        str.AppendLine("}");
        str.AppendLine();
    }

    private Union CreateUnion(INamedTypeSymbol typeSymbol)
    {
        var variants = CreateVariants(typeSymbol);

        var typeCounts = variants
            .SelectMany(v => v.ParameterTypeCount)
            .GroupBy(pair => pair.Key)
            .ToDictionary(group => group.Key, group => group.Max(pair => pair.Value.Item1));

        foreach (var variant in variants)
        {
            var typeCountsForVariant = new Dictionary<string, int>();
            foreach (var parameter in variant.Parameters)
            {
                if (variant.ParameterTypeCount[parameter.TypeNameInternal].Item1 <= 1)
                {
                    parameter.TypeNameWithIndex = $"{parameter.TypeNameInternal}0";
                    continue;
                }

                if (!typeCountsForVariant.ContainsKey(parameter.TypeNameInternal))
                    typeCountsForVariant[parameter.TypeNameInternal] = 0;

                parameter.TypeNameWithIndex = $"{parameter.TypeNameInternal}{typeCountsForVariant[parameter.TypeNameInternal]}";
                typeCountsForVariant[parameter.TypeNameInternal]++;
            }
        }

        var valueIdentifiers = new Dictionary<string, ParameterInternalValue>();

        var index = 0;
        foreach (var (key, count) in typeCounts.OrderByDescending(x => x.Key))
        {
            for (var i = 0; i < count; i++)
            {
                valueIdentifiers[$"{key}{i}"] = new ParameterInternalValue(key, $"T{index}{i}");
            }

            index++;
        }

        var union = new Union(typeSymbol, variants)
        {
            ValueIdenfifiers = valueIdentifiers
        };

        return union;
    }

    private UnionVariant[] CreateVariants(INamedTypeSymbol typeSymbol)
    {
        const string k_DefaultAttributeMetadataName = "pyr.Union.DefaultAttribute";

        var defaultAttribute = _compilation.GetTypeByMetadataName(k_DefaultAttributeMetadataName);

        var unionVariants = (from method in typeSymbol.GetMembers().OfType<IMethodSymbol>()
                where method.IsPartialDefinition
                      && !method.ReturnsVoid
                      && !method.IsGenericMethod
                where SymbolEqualityComparer.Default.Equals(method.ReturnType, typeSymbol)
                select new UnionVariant(0, typeSymbol, method)
            )
            .ToArray();

        unionVariants = unionVariants.OrderByDescending(t => t.MethodSymbol
                .GetAttributes()
                .Any(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, defaultAttribute)))
            .ThenBy(t => t.PascalName)
            .ToArray();

        var variants = unionVariants.Indexed().Select(x => x.value with { Index = x.index }).ToArray();
        return variants;
    }

    private record Union(INamedTypeSymbol TypeSymbol, UnionVariant[] Variants)
    {
        public Dictionary<string, ParameterInternalValue> ValueIdenfifiers { get; internal set; } = new();

        public string ValuePropertyName(string typeName)
        {
            return ValueIdenfifiers[typeName].TypePropertyName;
        }
    }

    private record UnionVariant(int Index, INamedTypeSymbol TypeSymbol, IMethodSymbol MethodSymbol)
    {
        public object Accessibility { get; } = MethodSymbol.DeclaredAccessibility.GetSourceText();
        public string PascalName { get; } = MethodSymbol.Name.ToPascalCase();
        public bool EmptyVariant { get; } = !MethodSymbol.Parameters.Any();
        public string KindProperty => $"{InternalKindName}.{MethodSymbol.Name.ToPascalCase()}";

        public Dictionary<string, (int, IParameterSymbol)> ParameterTypeCount { get; } =
            MethodSymbol.Parameters
                .GroupBy(ParameterSymbolHelper.FullTypeNameWithNullable)
                .ToDictionary(x => x.Key, x => (x.Count(), x.FirstOrDefault()));

        public UnionVariantParameter[] Parameters { get; } =
            MethodSymbol.Parameters.Select(param => new UnionVariantParameter(param)).ToArray();
    }

    private record UnionVariantParameter(IParameterSymbol ParameterSymbol)
    {
        public string RawName { get; } = ParameterSymbol.Name;
        public string PascalName { get; } = ParameterSymbol.Name.ToPascalCase();
        public string CamelName { get; } = ParameterSymbol.Name.ToCamelCase();
        public string TypeName { get; } = ParameterSymbol.Type.GetFullName() + GetNullableAnnotation(ParameterSymbol);

        public string TypeNameInternal { get; } = ParameterSymbolHelper.FullTypeNameWithNullable(ParameterSymbol);
        public string TypeNameWithIndex { get; internal set; } = ParameterSymbol.Type.GetFullName();
        private static string GetNullableAnnotation(IParameterSymbol param)
        {
            return param switch
            {
                { Type.IsValueType: false, NullableAnnotation: NullableAnnotation.Annotated } => "?",
                _ => string.Empty
            };
        }
    }

    private static class ParameterSymbolHelper
    {
        public static string FullTypeNameWithNullable(IParameterSymbol parameter)
        {
            return parameter.Type.GetFullName() + parameter switch
            {
                { Type.IsValueType: false } => "?",
                _ => string.Empty
            };
        }
    }

    private record ParameterInternalValue(string TypeName, string TypePropertyName);
}
