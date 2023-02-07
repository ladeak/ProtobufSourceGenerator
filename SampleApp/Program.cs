using ProtoBuf;
using SampleApp;

using var ms = new MemoryStream();
Serializer.Serialize(ms, new Entity() { Id = 1 });
ms.Seek(0, SeekOrigin.Begin);
var entity =  Serializer.Deserialize<Entity>(ms);
Console.WriteLine(entity.Id);