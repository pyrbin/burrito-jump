using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Unity.CodeEditor;
using UnityEditor;
using UnityEngine;

namespace pyr.Core.Editor;

public sealed class CSharpProjectModifier : AssetPostprocessor
{
    private const bool k_AddGloablUsingsForIncludedProjects = false;
    private static readonly string[] IncludedProjectsPrefix = { "pyr.Lib" };

    private static FileSystemWatcher? s_Watcher;

    private static string OnGeneratedCSProject(string path, string content)
    {
        var isProjectFiles =
            path.EndsWith("Assembly-CSharp.csproj")
            || path.EndsWith("Assembly-CSharp-Editor.csproj");
        var isEditorProjectFiles = path.EndsWith("Editor.csproj");
        var isOtherProjects = IncludedProjectsPrefix.Any(path.Contains) && path.EndsWith(".csproj");
        var addImplicitUsings = k_AddGloablUsingsForIncludedProjects || isProjectFiles;
        var canApply = isProjectFiles || isOtherProjects;

        if (!canApply)
            return content;

        // csc.rsp
        var baseDir = Path.GetDirectoryName(path);
        if (baseDir != null)
        {
            var cscRspPath = Path.Combine(baseDir, "Assets", "csc.rsp");
            const string cscRspContent = "-nullable:enable -langversion:preview";
            File.WriteAllText(cscRspPath, cscRspContent);
        }

        var xDoc = XDocument.Parse(content);
        var langVersion = xDoc.Descendants("LangVersion").FirstOrDefault();
        if (langVersion != null)
        {
            langVersion.Value = "preview";
            langVersion.AddAfterSelf(new XElement("Nullable", "enable"));
            langVersion.AddAfterSelf(new XElement("EnforceCodeStyleInBuild", "true"));
            if (addImplicitUsings) langVersion.AddAfterSelf(new XElement("ImplicitUsings", "enable"));
        }

        using StringWriter stringWriter = new();
        XmlWriterSettings settings =
            new()
            {
                Indent = true,
                OmitXmlDeclaration = true,
                NewLineHandling = NewLineHandling.Entitize
            };
        using (var xmlWriter = XmlWriter.Create(stringWriter, settings))
        {
            xDoc.Save(xmlWriter);
        }

        content = stringWriter.ToString();

        return content;
    }

    private static void OnCSProjectCreated(string file)
    {
        if (string.IsNullOrEmpty(file) || Path.GetExtension(file) != ".csproj" || !File.Exists(file))
            return;

        var codeEditor = CodeEditor.Editor.CurrentCodeEditor;
        if (codeEditor?.GetType().Name != "VSCodeScriptEditor")
            return;

        try
        {
            var text = File.ReadAllText(file, Encoding.UTF8);
            var beforeHash = text.GetHashCode();
            text = OnGeneratedCSProject(file, text);
            var afterHash = text.GetHashCode();
            if (beforeHash != afterHash) File.WriteAllText(file, text);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    [InitializeOnLoadMethod]
    private static void WatchCsProject()
    {
#if !UNITY_EDITOR_WIN
            Environment.SetEnvironmentVariable("MONO_MANAGED_WATCHER", "enabled");
#endif
        var resultDir = Path.GetFullPath(".");
        foreach (var file in Directory.GetFiles(resultDir, "*.csproj"))
            EditorApplication.delayCall += () => OnCSProjectCreated(Path.Combine(resultDir, file));

        s_Watcher?.Dispose();
        s_Watcher = new FileSystemWatcher
        {
            Path = resultDir,
            NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite,
            IncludeSubdirectories = false,
            EnableRaisingEvents = true,
            Filter = "*.csproj"
        };

        s_Watcher.Created += (s, e) => EditorApplication.delayCall += () => OnCSProjectCreated(e.FullPath);
        s_Watcher.Changed += (s, e) => EditorApplication.delayCall += () => OnCSProjectCreated(e.FullPath);
    }
}
