using ProtoBuf;

namespace SampleApp;

[ProtoContract]
public partial class Entity
{
    public int Id { get; set; }

    public string Value { get; set; }
}