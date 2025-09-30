using System.Text.Json.Serialization;
using visual_ssh.Models;

[JsonSerializable(typeof(List<Server>))]
internal partial class ServersJsonContext : JsonSerializerContext
{
}
