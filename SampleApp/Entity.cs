using ProtoBuf;

namespace SampleApp;

[ProtoContract]
public partial class Entity
{
    [ProtoContract]
    public partial class SomeEntity
    {
        public int Id { get; set; }
    }

    public int Id { get; set; }

    public string Value { get; set; }
}
