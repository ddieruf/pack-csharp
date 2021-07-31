using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace pack_csharp.Util
{
  public record BillOfMaterials([property: JsonPropertyName("remote")] IEnumerable<BomItem> Remote, [property: JsonPropertyName("local")] IEnumerable<BomItem> Local)
  {
    public string ToJson()
    {
      return JsonSerializer.Serialize(this);
    }

    public string GetHash()
    {
      return Hash.Create(ToJson());
    }

    public static BillOfMaterials FromJson(string json)
    {
      return JsonSerializer.Deserialize<BillOfMaterials>(json);
    }
  }

  public record BomItem([property: JsonPropertyName("name")] string Name, [property: JsonPropertyName("metadata")]
    Metadata Metadata, [property: JsonPropertyName("buildpacks")]
    BuildpackMeta Buildpacks);

  public record Metadata([property: JsonPropertyName("layer")] string Layer, [property: JsonPropertyName("names")] IEnumerable<string> Names, [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("version")]
    string Version, [property: JsonPropertyName("licenses")]
    IEnumerable<string> Licenses, [property: JsonPropertyName("sha256")] string Sha256, [property: JsonPropertyName("stacks")] IEnumerable<string> Stacks,
    [property: JsonPropertyName("uri")] string Uri, [property: JsonPropertyName("launch")] bool Launch = false);

  public record BuildpackMeta([property: JsonPropertyName("id")] string Id, [property: JsonPropertyName("version")]
    string Version);
}
