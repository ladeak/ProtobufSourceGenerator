using ProtoBuf;
using SampleApp;

using var ms = new MemoryStream();
Serializer.Serialize(ms, new InitEntity() { Id = 1 });
ms.Seek(0, SeekOrigin.Begin);
var entity = Serializer.Deserialize<InitEntity>(ms);
Console.WriteLine(entity.Id);