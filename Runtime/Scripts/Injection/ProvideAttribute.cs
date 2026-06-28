using System;
using UnityEngine;

namespace WorldShaper.Injection
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ProvideAttribute : PropertyAttribute { }
}