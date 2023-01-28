using ProtoBuf;

namespace SampleApp;

[ProtoContract]
public partial class CustomOrderedEntity
{
    [ProtoMember(1)]
    public int Id { get; set; }

    public List<string?> Value { get; set; } = new();
}
