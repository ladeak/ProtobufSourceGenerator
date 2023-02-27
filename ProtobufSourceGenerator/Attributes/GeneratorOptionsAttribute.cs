using System;

namespace ProtobufSourceGenerator
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public class GeneratorOptionsAttribute : Attribute
    {
        public Type PropertyAttributeType { get; set; }
    }
}
