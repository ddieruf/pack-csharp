using System;
using System.Collections.Generic;
using System.IO;

namespace pack_csharp.Util
{
  /// <summary>
  /// </summary>
  /// <param name="path">
  ///   Path to app dir or zip-formatted file (defaults to Environment.CurrentDirectory
  ///   <see cref="Environment.CurrentDirectory" />)
  /// </param>
  /// <param name="buildpack">
  ///   Buildpack(s) to use. One of:
  ///   a buildpack by id and version in the form of '&lt;buildpack&gt;@&lt;version&gt;',
  ///   path to a buildpack directory (not supported on Windows),
  ///   path/URL to a buildpack .tar or .tgz file,
  ///   or a packaged buildpack image name in the form of '&lt;hostname&gt;/&lt;repo&gt;[:&lt;tag&gt;]'
  ///   Provide a LinkedList of buildpacks in the order they should run.
  /// </param>
  /// <param name="buildpackRegistry">Buildpack Registry by name</param>
  /// <param name="cacheImage">Cache build layers in remote registry. Requires publish==true.</param>
  /// <param name="clearCache">Clear image's associated cache before building</param>
  /// <param name="builderImage"></param>
  /// <param name="defaultProcess">Set the default process type. (default "web")</param>
  /// <param name="descriptor">Path to the project descriptor file</param>
  /// <param name="dockerHost">
  ///   Address to docker daemon that will be exposed to the build container.
  ///   If not set the standard socket location will be used. Special value 'inherit' may be used in which case DOCKER_HOST
  ///   environment variable will be used.
  ///   This option may set DOCKER_HOST environment variable for the build container if needed.
  /// </param>
  /// <param name="env">
  ///   Build-time environment variable, in the form 'VAR, VALUE' or 'VAR, null'. When using latter value-less form, value
  ///   will be taken from current environment at the time
  ///   this command is executed. This flag may be specified multiple times and will override individual values defined by
  ///   'envFile'. Repeat for each env in order.
  ///   NOTE: These are NOT available at image runtime.
  /// </param>
  /// <param name="envFile">
  ///   Build-time environment variables file. One variable per line, of the form 'VAR=VALUE' or 'VAR' When using latter
  ///   value-less form, value will be taken from current
  ///   environment at the time this command is executed.
  ///   NOTE: These are NOT available at image runtime."
  /// </param>
  /// <param name="lifecycleImage">Custom lifecycle image to use for analysis, restore, and export when builder is untrusted.</param>
  /// <param name="network">Connect detect and build containers to network.</param>
  /// <param name="publish">Publish to registry</param>
  /// <param name="pullPolicy">Pull policy to use. Accepted values are always, never, and if-not-present. (default "always")</param>
  /// <param name="runImage">Run image (defaults to default stack's run image)</param>
  /// <param name="tags">Additional tags to push the output image to. Repeat for each tag in order.</param>
  /// <param name="volume">
  ///   Mount host volume into the build container, in the form '&lt;host path&gt;:&lt;target path&gt;[:&lt;options&gt;]'.
  ///   - 'host path': Name of the volume or absolute directory path to mount.
  ///   - 'target path': The path where the file or directory is available in the container.
  ///   - 'options' (default "ro"): An optional comma separated list of mount options.
  ///   - "ro", volume contents are read-only.
  ///   - "rw", volume contents are readable and writeable.
  ///   - "volume-opt=&lt;key&gt;=&lt;value&gt;", can be specified more than once, takes a key-value pair consisting of the
  ///   option name and its value.
  ///   Repeat for each volume in order.
  /// </param>
  /// <param name="workspace">Location at which to mount the app dir in the build image</param>
  public record PackBuildSpec(SortedSet<string> Buildpack = default, string BuildpackRegistry = default, string CacheImage = default, bool ClearCache = false, string DefaultProcess = default,
    FileInfo Descriptor = default, string DockerHost = default, IDictionary<string, string> Env = default, FileInfo EnvFile = default, int? Gid = default, string LifecycleImage = default,
    string Network = default, string Path = default, bool Publish = false, string PullPolicy = default, string RunImage = default, SortedSet<string> Tags = default, bool? TrustBuilder = default,
    SortedSet<string> Volume = default, string Workspace = default)
  {
    public IEnumerable<string> ToArgumentList()
    {
      var argumentsList = new List<string>().AddCliFlag("--buildpack", Buildpack).AddCliFlag("--buildpackRegistry", BuildpackRegistry).AddCliFlag("--cacheImage", CacheImage)
        .AddCliFlag("--defaultProcess", DefaultProcess).AddCliFlag("--dockerHost", DockerHost).AddCliFlag("--env", Env).AddCliFlag("--lifecycleImage", LifecycleImage).AddCliFlag("--network", Network)
        .AddCliFlag("--pullPolicy", PullPolicy).AddCliFlag("--runImage", RunImage).AddCliFlag("--tag", Tags).AddCliFlag("--volume", Volume).AddCliFlag("--workspace", Workspace);

      if (Gid.HasValue) argumentsList.AddCliFlag("--gid", Gid.Value.ToString());

      if (Publish)
        argumentsList.Add("--publish");

      if (ClearCache)
        argumentsList.Add("--clearCache");

      if (TrustBuilder is true)
        argumentsList.Add("--trust-builder");

      if (Descriptor != default) argumentsList.AddCliFlag("--descriptor", Descriptor.FullName);

      if (EnvFile != default) argumentsList.AddCliFlag("--envFile", EnvFile.FullName);

      return argumentsList;
    }
  }
}