using System;
using System.Collections;
using System.Collections.Generic;

namespace pack_csharp.Util
{
  /// <summary>
  ///   The spec that will be used to create an image of a artifact
  /// </summary>
  /// <param name="BuilderName">Builder image (default "cnbs/sample-builder:bionic")</param>
  /// <param name="ArtifactRepositoryId">Where to retrieve the artifact bytes</param>
  /// <param name="ImageName">The name of the image (should also include the registry address "save-to-this-repo/image_name")</param>
  /// <param name="EnvVars">Include these lable/values with writing container definition</param>
  /// <param name="Tags">Add these tags on the image</param>
  public record BuildpackSpec(string BuilderName, Guid ArtifactRepositoryId, string ImageName, IReadOnlyDictionary<string, string> EnvVars = default, IEnumerable Tags = default);
}