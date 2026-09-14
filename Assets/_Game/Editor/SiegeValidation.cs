#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

/// <summary>Mały, nieblokujący most do uruchamiania testów Siege z MCP/Edytora.</summary>
public static class SiegeValidation
{
    private sealed class Result
    {
        public string Name; public string Status; public string Message;
    }

    [MenuItem("Game/Siege/Run EditMode Tests")]
    public static void RunEditModeTests()
    {
        var results = new List<Result>();
        var allowed = new HashSet<string>(StringComparer.Ordinal) {
            "SiegeWaveTests", "SiegeArenaTests", "SiegeAiTests", "SiegeProgressionTests",
            "BombermanKitRebuildTests", "BombermanProgressionTests", "WaveDefinitionTests",
            "SharedCameraMathTests", "ProgressionApplierTests" };
        foreach (var type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(SafeTypes)
            .Where(t => allowed.Contains(t.Name) && t.IsClass))
        {
            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var method in methods)
            {
                var cases = method.GetCustomAttributes<TestCaseAttribute>().Cast<TestCaseAttribute>().ToList();
                var isTest = method.GetCustomAttributes<TestAttribute>().Any() || cases.Count > 0;
                if (!isTest || typeof(IEnumerator).IsAssignableFrom(method.ReturnType)) continue;
                if (cases.Count == 0) cases.Add(new TestCaseAttribute(Array.Empty<object>()));
                foreach (var testCase in cases) results.Add(RunCase(type, method, testCase));
            }
        }
        var passed = results.Count(r => r.Status == "Passed");
        var failed = results.Count(r => r.Status == "Failed");
        var path = Path.GetFullPath(Path.Combine("docs", "plans", "siege-test-results.xml"));
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        var xml = new StringBuilder("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<siege-tests passed=\"")
            .Append(passed).Append("\" failed=\"").Append(failed).Append("\">\n");
        foreach (var result in results)
            xml.Append("  <test name=\"").Append(Xml(result.Name)).Append("\" status=\"").Append(Xml(result.Status))
                .Append("\" message=\"").Append(Xml(result.Message)).Append("\" />\n");
        File.WriteAllText(path, xml.Append("</siege-tests>\n").ToString());
        AssetDatabase.Refresh();
        Debug.Log($"[SiegeValidation] Reflection run: {passed} passed, {failed} failed. Results: {path}");
    }

    private static Result RunCase(Type type, MethodInfo method, TestCaseAttribute testCase)
    {
        var result = new Result { Name = type.Name + "." + method.Name, Status = "Passed", Message = "" };
        try
        {
            var instance = Activator.CreateInstance(type);
            try
            {
                foreach (var setup in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(m => m.GetCustomAttributes<SetUpAttribute>().Any())) setup.Invoke(instance, null);
                var args = testCase.Arguments ?? Array.Empty<object>();
                method.Invoke(instance, args);
            }
            finally
            {
                foreach (var teardown in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(m => m.GetCustomAttributes<TearDownAttribute>().Any())) teardown.Invoke(instance, null);
            }
        }
        catch (Exception error)
        {
            result.Status = "Failed";
            result.Message = error is TargetInvocationException tie && tie.InnerException != null ? tie.InnerException.ToString() : error.ToString();
        }
        return result;
    }

    private static string Xml(string value) => System.Security.SecurityElement.Escape(value ?? "") ?? "";
    private static IEnumerable<Type> SafeTypes(Assembly assembly)
    {
        try { return assembly.GetTypes(); }
        catch (ReflectionTypeLoadException error) { return error.Types.Where(t => t != null); }
    }
}
#endif
