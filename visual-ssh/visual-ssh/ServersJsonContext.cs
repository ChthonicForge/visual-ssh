using System.Text.Json.Serialization;
using visualSSH;

[JsonSerializable(typeof(List<Server>))]
internal partial class ServersJsonContext : JsonSerializerContext
{
}
