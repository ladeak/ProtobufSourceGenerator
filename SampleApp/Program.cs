using ProtoBuf;
using ProtobufSourceGenerator;
using SampleApp;

using var ms = new MemoryStream();
Serializer.Serialize(ms, new Entity() { Id = 1 });
ms.Seek(0, SeekOrigin.Begin);
var entity = Serializer.Deserialize<Entity>(ms);
Console.WriteLine(entity.Id);

using var ms2 = new MemoryStream();
Serializer.Serialize(ms2, new CustomOrderedEntity() { Id = 1, Value = new() });
ms2.Seek(0, SeekOrigin.Begin);
var customEntity = Serializer.Deserialize<CustomOrderedEntity>(ms2);
Console.WriteLine(customEntity.Value!.Count());

using var ms3 = new MemoryStream();
Serializer.Serialize(ms3, new B() { Value = 1, Data = "a" });
ms3.Seek(0, SeekOrigin.Begin);
var aaaa = Serializer.Deserialize<B>(ms3);
Console.WriteLine(aaaa.Value);
Console.WriteLine(aaaa.Data);

[ProtoContract]
[ProtoInclude(2, typeof(B))]
[GeneratorOptions(PropertyAttributeType = typeof(ObsoleteAttribute))]
public class A
{
    [ProtoMember(1)]
    public int Value { get; set; }
}

[ProtoContract]
public class B : A
{
    [ProtoMember(1)]
    public string Data { get; set; }
}