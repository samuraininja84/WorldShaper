using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace WorldShaper.Editor
{
    public static class IBehaviourTypeCache
    {
        private static IEnumerable<Assembly> assemblies = null;
        private static IEnumerable<Type> types = null;

        private static readonly HashSet<string> internalAssemblyPrefixes = new()
        {
            "Unity.",
            "UnityEditor.",
            "UnityEngine.",
            "JetBrains.",
            "System.",
            "Microsoft.",
            "Mono.",
            "ICSharpCode.",
            "Newtonsoft."
        };

        private static readonly HashSet<string> internalAssemblyNames = new()
        {
            "Bee.BeeDriver",
            "ExCSS.Unity",
            "Mono.Security",
            "mscorlib",
            "netstandard",
            "Newtonsoft.Json",
            "nunit.framework",
            "ReportGeneratorMerged",
            "Unrelated",
            "SyntaxTree.VisualStudio.Unity.Bridge",
            "SyntaxTree.VisualStudio.Unity.Messaging"
        };

        public static IEnumerable<Assembly> GetUserCreatedAssemblies(this AppDomain appDomain)
        {
            // Iterate through all assemblies in the AppDomain
            foreach (var assembly in appDomain.GetAssemblies())
            {
                // Skip dynamic assemblies
                if (assembly.IsDynamic) continue;

                // Get the assembly name
                string assemblyName = assembly.GetName().Name;

                // Skip editor assemblies
                if (assemblyName.Contains("Editor")) continue;

                // Skip internal/system assemblies by prefix
                if (internalAssemblyPrefixes.Any(prefix => assemblyName.Contains(prefix))) continue;

                // Skip internal/system assemblies
                if (internalAssemblyNames.Contains(assemblyName)) continue;

                // Yield return user-created assembly
                yield return assembly;
            }
        }

        public static IEnumerable<Type> GetEvaluatedInterfaces(this Assembly assembly)
        {
            // Helper method to check if a type implements a specific interface
            static bool HasInterface(Type type) => type.GetInterfaces().Any(i => i == typeof(IBehaviour)) && typeof(MonoBehaviour).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract;

            // Get all types that implement the interface in the assembly
            return assembly.GetTypes().Where(HasInterface);
        }

        public static IEnumerable<Assembly> GetEvaluatedAssemblies() => AppDomain.CurrentDomain.GetUserCreatedAssemblies().Where(assembly => assembly.GetEvaluatedInterfaces().Any());

        public static IEnumerable<Type> GetTypesWithIBehaviour()
        {
            // Check if the types have already been cached, and if so, return the cached types
            if (types != null) return types;

            // Get all user-created assemblies
            assemblies ??= AppDomain.CurrentDomain.GetUserCreatedAssemblies();

            // Iterate through each assembly to find methods with the attribute
            foreach (var assembly in assemblies)
            {
                // Get types that implement the IBehaviour interface in the current assembly
                var assemblyTypes = assembly.GetEvaluatedInterfaces();

                // If there are no types in this assembly, continue to the next
                if (!assemblyTypes.Any()) continue;

                // Accumulate types from all assemblies
                if (types == null) types = assemblyTypes;
                else types = types.Concat(assemblyTypes);
            }

            // Get all types that implement the specified interface from the provided assemblies
            return types;
        }
    }
}