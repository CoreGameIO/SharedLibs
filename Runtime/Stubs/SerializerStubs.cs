// Stub attributes so fp.cs compiles without real serializer packages.
// In Unity: excluded when real packages are present via versionDefines in .asmdef.
// In .NET NuGet builds: excluded from compilation entirely (real packages referenced).

#if !HAS_MEMORYPACK

namespace MemoryPack
{
    using System;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
    internal class MemoryPackableAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    internal class MemoryPackOrderAttribute : Attribute
    {
        public MemoryPackOrderAttribute(int order) { }
    }

}

#endif

#if !HAS_MESSAGEPACK

namespace MessagePack
{
    using System;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
    internal sealed class MessagePackObjectAttribute : Attribute
    {
        public bool KeyAsPropertyName { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    internal sealed class KeyAttribute : Attribute
    {
        public int IntKey { get; }
        public KeyAttribute(int x) => IntKey = x;
    }
}

#endif
