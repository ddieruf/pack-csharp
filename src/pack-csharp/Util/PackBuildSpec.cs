using System.Collections.Generic;
using System.IO;
using pack_csharp.Util;

namespace pack_csharp_tests.Util
{
  public record PackBuildSpec(string ImageName, string Builder, LinkedList<string> Buildpack = default, string BuildpackRegistry = default, string CacheImage = default, bool ClearCache = false,
    string DefaultProcess = default, FileInfo Descriptor = default, string DockerHost = default, IDictionary<string, string> Env = default, FileInfo EnvFile = default, int? Gid = default,
    string LifecycleImage = default, string Network = default, string Path = default, bool Publish = false, string PullPolicy = default, string RunImage = default, LinkedList<string> Tags = default,
    bool? TrustBuilder = default, LinkedList<string> Volume = default, string Workspace = default, bool DryRun = false)
  {
    public string ImageName => Ensure.NotNull(ImageName, nameof(ImageName));
  }
}
