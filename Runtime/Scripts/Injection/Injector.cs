using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace WorldShaper.Injection
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-1000)]
    [AddComponentMenu("World Shaper/World Shaper Injector")]
    public class Injector : PersistentSingleton<Injector> 
    {
        private readonly Dictionary<Type, object> registry = new();

        private const BindingFlags k_bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        protected override void OnInit()
        {
            // Find all MonoBehaviour instances in the scene
            var monoBehaviours = FindMonoBehaviours();
            
            // Find all modules implementing IDependencyProvider and register the dependencies they provide
            var providers = monoBehaviours.OfType<IDependencyProvider>();
            foreach (var provider in providers) Register(provider);

            // Find all injectable objects and inject their dependencies
            var injectables = monoBehaviours.Where(IsInjectable);
            foreach (var injectable in injectables) Inject(injectable);
        }

        /// <summary>
        /// Registers a specific instance of a type in the dependency registry.
        /// </summary>
        /// <remarks>This allows for manual registration of dependencies that may not be provided by any IDependencyProvider.</remarks>
        /// <typeparam name="T">The type of the instance to register.</typeparam>
        /// <param name="instance">The instance to register.</param>
        public void Register<T>(T instance) => registry[typeof(T)] = instance;

        /// <summary>
        /// Registers all dependencies provided by the given IDependencyProvider instance.
        /// </summary>
        /// <param name="provider">The IDependencyProvider instance to register dependencies from.</param>
        /// <exception cref="Exception">Thrown if a provider method returns null.</exception>
        private void Register(IDependencyProvider provider)
        {
            // Get all methods of the provider that are marked with the Provide attribute
            var methods = provider.GetType().GetMethods(k_bindingFlags);

            // Iterate through each method and register the provided instance in the registry
            foreach (var method in methods) 
            {
                // Check if the method is marked with the Provide attribute
                if (!Attribute.IsDefined(method, typeof(ProvideAttribute))) continue;

                // Get the return type of the method
                var returnType = method.ReturnType;

                // Invoke the provider method to get the provided instance
                var providedInstance = method.Invoke(provider, null);

                // If the provided instance is not null, add it to the registry; otherwise, throw an exception
                if (providedInstance != null) 
                {
                    // Add the provided instance to the registry with its return type as the key
                    registry.Add(returnType, providedInstance);
                } 
                else 
                {
                    // If the provided instance is null, log an error indicating that the provider method returned null
                    Debug.LogError($"Provider method '{method.Name}' in class '{provider.GetType().Name}' returned null when providing type '{returnType.Name}'.");
                }
            }
        }

        /// <summary>
        /// Injects dependencies into the specified instance by resolving and setting values for fields, properties, and methods marked with the Inject attribute.
        /// </summary>
        /// <param name="instance">The instance to inject dependencies into.</param>
        private void Inject(object instance)
        {
            // Get the type of the instance to inject dependencies into
            var type = instance.GetType();

            // Inject dependencies into fields marked with the Inject attribute
            InjectFields(instance, type);

            // Inject dependencies into methods marked with the Inject attribute
            InjectMethods(instance, type);

            // Inject dependencies into properties marked with the Inject attribute
            InjectProperties(instance, type);
        }

        /// <summary>
        /// Injects dependencies into fields of the specified instance that are marked with the Inject attribute.
        /// </summary>
        /// <param name="instance">The instance to inject dependencies into.</param>
        /// <param name="type">The type of the instance to inject dependencies into.</param>
        /// <exception cref="Exception">Thrown if a dependency cannot be resolved.</exception>
        private void InjectFields(object instance, Type type)
        {
            // Collect all fields marked with the Inject attribute and inject dependencies into them
            var injectableFields = type.GetFields(k_bindingFlags).Where(member => Attribute.IsDefined(member, typeof(InjectAttribute)));

            // Inject dependencies into each injectable field
            foreach (var injectableField in injectableFields)
            {
                // Check if the field is already set; if it is, log a warning and skip injection for that field
                if (injectableField.GetValue(instance) != null)
                {
                    // Log a warning message indicating that the field is already set and will be skipped
                    Debug.LogWarning($"[Injector] Field '{injectableField.Name}' of class '{type.Name}' is already set.");

                    // Skip to the next field if the current field is already set
                    continue;
                }

                // Get the type of the field to be injected
                var fieldType = injectableField.FieldType;

                // If we cannot resolve the dependency for the field type, log an error
                var resolvedInstance = Resolve(fieldType);

                // If the resolved instance is null, log an error indicating that the injection failed for the field
                if (resolvedInstance == null)
                {
                    // Log an error message indicating that the injection failed for the field
                    Debug.LogError($"Failed to inject dependency into field '{injectableField.Name}' of class '{type.Name}'.");

                    // Skip to the next field if the current field's dependency cannot be resolved
                    continue;
                }

                // Set the value of the injectable field to the resolved instance
                injectableField.SetValue(instance, resolvedInstance);
            }
        }

        /// <summary>
        /// Injects dependencies into methods of the specified instance that are marked with the Inject attribute.
        /// </summary>
        /// <param name="instance">The instance to inject dependencies into.</param>
        /// <param name="type">The type of the instance to inject dependencies into.</param>
        /// <exception cref="Exception">Thrown if a dependency cannot be resolved.</exception>
        private void InjectMethods(object instance, Type type)
        {
            // Collect all methods marked with the Inject attribute and inject dependencies into them
            var injectableMethods = type.GetMethods(k_bindingFlags).Where(member => Attribute.IsDefined(member, typeof(InjectAttribute)));

            // Inject dependencies into each injectable method
            foreach (var injectableMethod in injectableMethods)
            {
                // Get the required parameter types for the injectable method
                var requiredParameters = injectableMethod.GetParameters().Select(parameter => parameter.ParameterType).ToArray();

                // Get the resolved instances for each required parameter type
                var resolvedInstances = requiredParameters.Select(Resolve).ToArray();

                // Check if any of the resolved instances are null, indicating a failed dependency resolution
                if (resolvedInstances.Any(resolvedInstance => resolvedInstance == null))
                {
                    // If any of the resolved instances are null, log an error indicating that the injection failed for the method
                    Debug.LogError($"Failed to inject dependencies into method '{injectableMethod.Name}' of class '{type.Name}'.");

                    // Skip to the next method if the current method's dependencies cannot be resolved
                    continue;
                }

                // Invoke the injectable method with the resolved instances as parameters
                injectableMethod.Invoke(instance, resolvedInstances);
            }
        }

        /// <summary>
        /// Injects dependencies into properties of the specified instance that are marked with the Inject attribute.
        /// </summary>
        /// <param name="instance">The instance to inject dependencies into.</param>
        /// <param name="type">The type of the instance to inject dependencies into.</param>
        /// <exception cref="Exception">Thrown if a dependency cannot be resolved.</exception>
        private void InjectProperties(object instance, Type type)
        {
            // Collect all properties marked with the Inject attribute and inject dependencies into them
            var injectableProperties = type.GetProperties(k_bindingFlags).Where(member => Attribute.IsDefined(member, typeof(InjectAttribute)));

            // Inject dependencies into each injectable property
            foreach (var injectableProperty in injectableProperties)
            {
                // Get the type of the property to be injected
                var propertyType = injectableProperty.PropertyType;

                // Check if the property has a setter; if not, log an error and skip injection
                if (!injectableProperty.CanWrite)
                {
                    // Log an error message indicating that the property cannot be injected due to the lack of a setter
                    Debug.LogError($"[Injector] Property '{injectableProperty.Name}' of class '{type.Name}' does not have a setter and cannot be injected.");

                    // Skip to the next property if the current property cannot be injected
                    continue;
                }

                // If we cannot resolve the dependency for the property type, log an error
                var resolvedInstance = Resolve(propertyType);

                // If the resolved instance is null, log an error indicating that the injection failed for the property
                if (resolvedInstance == null)
                {
                    // Log an error message indicating that the injection failed for the property
                    Debug.LogError($"Failed to inject dependency into property '{injectableProperty.Name}' of class '{type.Name}'.");

                    // Skip to the next property if the current property's dependency cannot be resolved
                    continue;
                }

                // Set the value of the injectable property to the resolved instance
                injectableProperty.SetValue(instance, resolvedInstance);
            }
        }

        /// <summary>
        /// Validates the dependencies of all MonoBehaviour instances in the scene.
        /// </summary>
        public void ValidateDependencies()
        {
            // Find all MonoBehaviour instances in the scene
            var monoBehaviours = FindMonoBehaviours();

            // Find all modules implementing IDependencyProvider
            var providers = monoBehaviours.OfType<IDependencyProvider>();

            // Get the set of provided dependencies from the registered providers
            var providedDependencies = GetProvidedDependencies(providers);

            // Find all MonoBehaviour instances that have fields marked with the Inject attribute and check if they are missing any dependencies
            var invalidDependencies = monoBehaviours
                .SelectMany(mb => mb.GetType().GetFields(k_bindingFlags), (mb, field) => new {mb, field})
                .Where(t => Attribute.IsDefined(t.field, typeof(InjectAttribute)))
                .Where(t => !providedDependencies.Contains(t.field.FieldType) && t.field.GetValue(t.mb) == null)
                .Select(t => $"[Validation] {t.mb.GetType().Name} is missing dependency {t.field.FieldType.Name} on GameObject {t.mb.gameObject.name}");

            // Convert the invalid dependencies to a list for logging
            var invalidDependencyList = invalidDependencies.ToList();

            // Log the results of the validation
            if (!invalidDependencyList.Any()) 
            {
                // Log a message indicating that all dependencies are valid
                Debug.Log("[Validation] All dependencies are valid.");
            } 
            else 
            {
                // Log the number of invalid dependencies found
                Debug.LogError($"[Validation] {invalidDependencyList.Count} dependencies are invalid:");

                // Log each invalid dependency
                foreach (var invalidDependency in invalidDependencyList) Debug.LogError(invalidDependency);
            }
        }

        /// <summary>
        /// Clears all injectable fields in all MonoBehaviour instances in the scene by setting their values to null.
        /// </summary>
        public void ClearDependencies()
        {
            // Clear all injectable fields in all MonoBehaviour instances
            foreach (var monoBehaviour in FindMonoBehaviours()) 
            {
                // Get the type of the MonoBehaviour
                var type = monoBehaviour.GetType();

                // Find all fields marked with the Inject attribute
                var injectableFields = type.GetFields(k_bindingFlags).Where(member => Attribute.IsDefined(member, typeof(InjectAttribute)));

                // Clear the values of all injectable fields
                foreach (var injectableField in injectableFields) injectableField.SetValue(monoBehaviour, null);
            }

            // Log a message indicating that all injectable fields have been cleared
            Debug.Log("[Injector] All injectable fields cleared.");
        }

        /// <summary>
        /// Resolves a dependency of the specified type from the registry. If the type is not registered, returns null.
        /// </summary>
        /// <param name="type">The type of the dependency to resolve.</param>
        /// <returns>The resolved instance of the specified type, or null if the type is not registered.</returns>
        private object Resolve(Type type) => registry.TryGetValue(type, out var resolvedInstance) ? resolvedInstance : null;

        /// <summary>
        /// Gets the set of provided dependencies from the given collection of IDependencyProvider instances.
        /// </summary>
        /// <param name="providers">The collection of IDependencyProvider instances to get dependencies from.</param>
        /// <returns>A set of types representing the provided dependencies.</returns>
        private HashSet<Type> GetProvidedDependencies(IEnumerable<IDependencyProvider> providers)
        {
            // Create a set to hold the types of dependencies provided by the providers
            var providedDependencies = new HashSet<Type>();

            // Iterate through each provider and collect the types of dependencies they provide
            foreach (var provider in providers) 
            {
                // Get all methods of the provider that are marked with the Provide attribute
                var methods = provider.GetType().GetMethods(k_bindingFlags);

                // Iterate through each method and add its return type to the set of provided dependencies
                foreach (var method in methods) 
                {
                    // Check if the method is marked with the Provide attribute
                    if (!Attribute.IsDefined(method, typeof(ProvideAttribute))) continue;

                    // Get the return type of the method
                    var returnType = method.ReturnType;

                    // Add the return type to the set of provided dependencies
                    providedDependencies.Add(returnType);
                }
            }

            // Return the set of provided dependencies
            return providedDependencies;
        }

        /// <summary>
        /// Finds all MonoBehaviour instances in the scene using Unity's FindObjectsByType method with InstanceID sorting.
        /// </summary>
        /// <returns>An array of all MonoBehaviour instances in the scene.</returns>
        private static MonoBehaviour[] FindMonoBehaviours() => FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.InstanceID);

        /// <summary>
        /// Determines whether the specified MonoBehaviour instance has any members (fields, properties, or methods) marked with the Inject attribute, indicating that it is injectable.
        /// </summary>
        /// <param name="obj">The MonoBehaviour instance to check for injectability.</param>
        /// <returns>True if the MonoBehaviour instance has any members marked with the Inject attribute; otherwise, false.</returns>
        private static bool IsInjectable(MonoBehaviour obj)
        {
            // Get all members of the object, including private and public fields, properties, and methods
            var members = obj.GetType().GetMembers(k_bindingFlags);

            // Check if any member has the Inject attribute
            return members.Any(member => Attribute.IsDefined(member, typeof(InjectAttribute)));
        }
    }
}