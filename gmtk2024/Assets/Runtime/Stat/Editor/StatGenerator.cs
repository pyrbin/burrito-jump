using System.Security.Cryptography;
using System.Text;
using gmtk2024.Editor.CodeGen;
using UnityCodeGen;
using UnityEditor;

namespace gmtk2024.Runtime.Stat.Editor.CodeGen;

[Generator]
public class StatGenerator : ICodeGenerator
{
    public void Execute(GeneratorContext context)
    {
        var path = $"{Constants.AssetsFolder}/Runtime/{Constants.GeneratedFolder}/Stat";
        var stats = StatResolver.GetStatsFromTypeCache();
        stats.Sort(static (a, b) => a.Name.CompareTo(b.Name));

        var str = new StringBuilder();
        GenerateHeader(str);
        GenerateNamespace(str);
        var code = GenerateStatType(str, stats);
        context.OverrideFolderPath(path);
        context.AddCode(string.Format(Constants.OutputFileFormat, "Stat"), code);
    }

    private static void GenerateHeader(StringBuilder str)
    {
        var self = typeof(StatGenerator);
        str.AppendFormat(
            Constants.AutoGenHeaderFormat,
            $"{self.Namespace.Replace($"{Constants.RootNamespace}.", "").Replace(".", "/")}/{self.Name}.cs",
            $"{self.Namespace.Replace($"{Constants.RootNamespace}.", "").Replace(".", "/")}/{self.Name}.cs",
            Constants.GetAssemblyFileName(self)
        );
        str.AppendLine("");
    }

    private static void GenerateNamespace(StringBuilder str)
    {
        str.AppendLine("namespace gmtk2024.Runtime.Stat;");
        str.AppendLine("");
    }

    private static string GenerateStatType(StringBuilder str, List<StatTypeInfo> stats)
    {
        str.AppendLine("[System.Serializable]");
        str.AppendLine("public enum StatType : i32");
        str.AppendLine("{");

        foreach (var stat in stats)
            str.AppendLine($"    {stat.Name} = {stat.TypeUuid},");

        str.AppendLine("}");

        str.AppendLine();
        str.AppendLine("public static class StatTypeExtensions");
        str.AppendLine("{");
        str.AppendLine("    public static Type ToType(this StatType statType)");
        str.AppendLine("    {");
        str.AppendLine("        return statType switch");
        str.AppendLine("        {");

        foreach (var stat in stats)
            str.AppendLine($"            StatType.{stat.Name} => typeof({stat.FullName}),");

        str.AppendLine(
            "            _ => throw new System.ArgumentException($\"Unknown StatType: {statType}\"),"
        );
        str.AppendLine("        };");
        str.AppendLine("    }");

        str.AppendLine();
        str.AppendLine("    public static StatType ToStatType(this Type type)");
        str.AppendLine("    {");
        str.AppendLine(
            "        if (type == null) throw new System.ArgumentNullException(nameof(type));"
        );

        foreach (var stat in stats)
            str.AppendLine(
                $"        if (type == typeof({stat.FullName})) return StatType.{stat.Name};"
            );

        str.AppendLine("        throw new ArgumentException($\"Unknown type: {type}\");");
        str.AppendLine("    }");
        str.AppendLine("}");

        return str.ToString();
    }
}

internal static class StatResolver
{
    public static List<StatTypeInfo> GetStatsFromTypeCache()
    {
        var types = TypeCache.GetTypesWithAttribute<StatAttribute>();
        var stats = new List<StatTypeInfo>();

        foreach (var type in types)
            stats.Add(
                new StatTypeInfo
                {
                    Name = type.Name,
                    Namespace = type.Namespace,
                    TypeUuid = TypeUuid(type.FullName!)
                }
            );

        return stats;
    }

    private static i32 TypeUuid(string name)
    {
        var bytes = Encoding.UTF8.GetBytes(name);
        using var sha1 = SHA1.Create();
        var result = sha1.ComputeHash(bytes);
        var hash = BitConverter.ToUInt16(result, 0);
        return hash;
    }
}

internal record struct StatTypeInfo
{
    public string Name { get; init; }
    public string? Namespace { get; init; }

    public i32 TypeUuid { get; init; }

    public string FullName => $"{Namespace}.{Name}";
}
