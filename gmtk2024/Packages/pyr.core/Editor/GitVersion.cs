using System.Diagnostics;
using pyr.Union.Monads;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using static pyr.Union.Global;

namespace pyr.Core.Editor;

public sealed class GitVersion : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    public void OnPostprocessBuild(BuildReport report)
    {
        UpdateVersion();
    }

    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        UpdateVersion();
    }

    [InitializeOnLoadMethod]
    public static void Initialize()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange change)
    {
        UpdateVersion();
    }

    private static void UpdateVersion()
    {
        var semver = GitSemver();
        var hash = GitCommitHash();
        var version = $"{semver}-{hash}";

        PlayerSettings.bundleVersion = version;
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static Semver? GitSemver()
    {
        var psi = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = "describe --tags --abbrev=0",
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        using var process = Process.Start(psi);

        using var reader = process!.StandardOutput;
        var result = reader.ReadToEnd().Trim();
        return Semver.From(result).IsSome(out var semver) ? semver : default;
    }

    private static string GitCommitHash()
    {
        var psi = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = "rev-parse --short HEAD",
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        using var process = Process.Start(psi);
        using var reader = process!.StandardOutput;
        return reader.ReadToEnd().Trim();
    }
}

public struct Semver
{
    public int Major;
    public int Minor;
    public int Patch;
    public string? PreReleaseIdentifier;

    public static Option<Semver> From(string versionString)
    {
        Semver semver = new();
        var components = versionString.Split('.', '-');
        if (components.Length < 3)
            return None;
        if (!int.TryParse(components[0], out semver.Major))
            return None;
        if (!int.TryParse(components[1], out semver.Minor))
            return None;
        if (!int.TryParse(components[2], out semver.Patch))
            return None;
        if (components.Length > 3)
            semver.PreReleaseIdentifier = components[3];
        return semver;
    }

    public readonly override string ToString()
    {
        return string.IsNullOrEmpty(PreReleaseIdentifier)
            ? $"{Major}.{Minor}.{Patch}"
            : $"{Major}.{Minor}.{Patch}-{PreReleaseIdentifier}";
    }
}
