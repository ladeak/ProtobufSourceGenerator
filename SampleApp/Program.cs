using ProtoBuf;
using SampleApp;

using var ms = new MemoryStream();
Serializer.Serialize(ms, new Entity() { Id = 1 });
ms.Seek(0, SeekOrigin.Begin);
var entity =  Serializer.Deserialize<Entity>(ms);
Console.WriteLine(entity.Id);

using var ms2 = new MemoryStream();
Serializer.Serialize(ms2, new CustomOrderedEntity() { Id = 1, Value = new() });
ms2.Seek(0, SeekOrigin.Begin);
var customEntity = Serializer.Deserialize<CustomOrderedEntity>(ms2);
Console.WriteLine(customEntity.Value!.Count());