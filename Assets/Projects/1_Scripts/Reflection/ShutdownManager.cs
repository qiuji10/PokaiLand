#if ENABLE_SHUTDOWN
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace PokaiLand.Setup
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class RuntimeShutdownMethodAttribute : Attribute
    {
    }

    public static class ShutdownManager
    {
        [RuntimeInitializeOnLoadMethod]
        private static void RegisterShutdownMethods()
        {
            Application.quitting += OnApplicationQuit;
        }

        private static void OnApplicationQuit()
        {
            var methods = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .SelectMany(type => type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                .Where(method => method.GetCustomAttribute<RuntimeShutdownMethodAttribute>() != null);

            foreach (var method in methods)
            {
                try
                {
                    method.Invoke(null, null);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error invoking shutdown method {method.Name}: {e.Message}");
                }
            }
        }
    }
}
#endif