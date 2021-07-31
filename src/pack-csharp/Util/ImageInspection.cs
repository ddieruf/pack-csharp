using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace pack_csharp
{
  public record ImageInspection(
    [property:JsonPropertyName("image_name")]
    string ImageName,
    [property:JsonPropertyName("remote_info")]
    RemoteInfo RemoteInfo,
    [property:JsonPropertyName("local_info")]
    LocalInfo LocalInfo
  )
  {
    public string ToJson()
    {
      return JsonSerializer.Serialize(this);
    }

    public string GetHash()
    {
      return Hash.Create(ToJson());
    }

    public static ImageInspection FromJson(string json)
    {
      return JsonSerializer.Deserialize<ImageInspection>(json);
    }
  }

  public record RemoteInfo();

  public record LocalInfo(
    [property:JsonPropertyName("stack")]
    string Stack,
    [property:JsonPropertyName("base_image")]
    IReadOnlyDictionary<string,string> BaseImage,
    [property:JsonPropertyName("run_images")]
    IEnumerable<IReadOnlyDictionary<string,string>> RunImages,
    [property:JsonPropertyName("buildpacks")]
    IEnumerable<IReadOnlyDictionary<string,string>> Buildpacks,
    [property:JsonPropertyName("processes")]
    IEnumerable<BuildpackProcess> Processes
  );

  public record BuildpackProcess(
    [property:JsonPropertyName("type")]
    string ProcessType,
    [property:JsonPropertyName("shell")]
    string Shell,
    [property:JsonPropertyName("command")]
    string Command,
    [property:JsonPropertyName("default")]
    bool Default,
    [property:JsonPropertyName("args")]
    string Args
  );
}
