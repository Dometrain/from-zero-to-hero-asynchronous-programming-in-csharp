using System.Text.Json.Serialization;

namespace HackerNews;

[JsonSerializable(typeof(IReadOnlyList<long>))]
[JsonSerializable(typeof(StoryModel))]
partial class HackerNewsJsonSerializerContext : JsonSerializerContext
{
}
