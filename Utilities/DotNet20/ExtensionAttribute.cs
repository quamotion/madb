#if !DOTNET35
using System;

namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Attribute used by the compiler to create extension methods under .NET 2.0.
    /// </summary>
    [AttributeUsageAttribute(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method)]
    public class ExtensionAttribute : Attribute
    {
    }
}
#endif