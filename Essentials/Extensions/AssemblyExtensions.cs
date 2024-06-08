using n_ate.Essentials.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace n_ate.Essentials
{
    public static class AssemblyExtensions
    {
        public static readonly Regex PackageReferenceRegEx = new Regex("\\<PackageReference +Include=\"(?<key>[^\\\"]+)\" +Version=\"(?<value>[^\\\"]+)\" */\\>");

        public static DependencyReferences? GetDepencyReferences(this Assembly assembly)
        {
            //var result = new Dictionary<string, string>();
            var fileName = assembly.GetName().Name + ".deps.json";
            var root = assembly.Location.Split("\\bin\\").First();
            if (root == assembly.Location) root = Directory.GetCurrentDirectory();
            var files = Directory.GetFiles(root, fileName, SearchOption.AllDirectories);
            if (files.Any())
            {
                var json = File.ReadAllText(files.First());
                var dependencies = JsonSerializer.Deserialize<DependencyReferences>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return dependencies;
            }
            return null;
        }

        public static Type[] GetImplementorsOf<TInterface>(this Assembly assembly, bool includeAbstractTypes = false)
                    where TInterface : class
        {
            var type = typeof(TInterface);
            if (!type.IsInterface) throw new ArgumentException($"{nameof(TInterface)} must be interface.");
            IEnumerable<Type> matches = assembly.GetTypes().Where(t => t.GetInterface(type.Name) is not null);
            if (!includeAbstractTypes) matches = matches.Where(t => !t.IsAbstract);
            return matches.ToArray();
        }

        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException("assembly");
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types?.Where(t => t is not null).Cast<Type>() ?? [];
            }
        }

        public static IDictionary<string, string> GetPackageReferencesFromCsproj(this Assembly assembly)
        {
            var result = new Dictionary<string, string>();
            var fileName = assembly.GetName().Name + ".csproj";
            var text = File.ReadAllText(fileName);
            var matches = PackageReferenceRegEx.Matches(text);
            foreach (Match match in matches)
            {
                result.Add(match.Groups["key"].Value, match.Groups["value"].Value);
            }
            return result;
        }

        public static Type[] GetSubclassesOf<TClass>(this Assembly assembly, bool includeAbstractTypes = false)
                            where TClass : class
        {
            var type = typeof(TClass);
            IEnumerable<Type> matches = assembly.GetTypes().Where(t => t.IsSubclassOf(type));
            if (!includeAbstractTypes) matches = matches.Where(t => !t.IsAbstract);
            return matches.ToArray();
        }

        public static Type[] GetTypesWithAttribute<TAttribute>(this Assembly assembly, bool includeAbstractTypes = false)
            where TAttribute : Attribute
        {
            var type = typeof(TAttribute);
            var usage = type.GetCustomAttribute<AttributeUsageAttribute>();
            var inherited = usage?.Inherited ?? false; //TODO: determine if recursively checking parent classes is necessary if flagged with "inherited"

            IEnumerable<Type> matches = assembly.GetTypes();
            if (!includeAbstractTypes) matches = matches.Where(t => !t.IsAbstract);
            matches = matches.Where(t => t.GetCustomAttribute<TAttribute>() is not null);
            return matches.ToArray();
        }
    }
}