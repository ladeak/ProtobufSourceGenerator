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


[ProtoContract]
public partial class InitEntity
{
    public int Id { get; init; }
}

[ProtoContract]
public partial class CustomOrderedEntity
{
    [ProtoMember(1)]
    public int Id { get; set; }

    public List<string>? Value { get; set; }

    public Dictionary<int, string>? Dictionary { get; set; }
}
