using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using pack_csharp.Runner;
using pack_csharp.Util;

namespace pack_csharp
{
  /// <summary>
  ///   For building apps using Cloud Native Buildpacks
  /// </summary>
  public class Pack
  {
    private readonly CancellationToken _cancellationToken;
    private readonly ILogger _log;
    private readonly bool _quiet;
    private readonly bool _timeStamps;
    private readonly bool _verbose;

    private string _packPath;

    /// <summary>
    ///   Generate app image from source code
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="log">A logger</param>
    /// <param name="quiet">Show less output</param>
    /// <param name="verbose">Show more output</param>
    /// <param name="timeStamps">Enable timestamps in output</param>
    /// <remarks>
    ///   The pack binaries are embedded in this library. During construction of this object the correct binary is matched to
    ///   the operating system and written to
    ///   <see cref="Environment.CurrentDirectory" />. Make sure the context running this library has permission.
    /// </remarks>
    public Pack(CancellationToken cancellationToken, ILogger log, bool quiet = false, bool verbose = false, bool timeStamps = false)
    {
      _quiet = quiet;
      _verbose = verbose;
      _timeStamps = timeStamps;
      _cancellationToken = cancellationToken;
      _log = log;

      Task.Run(async () => { _packPath = await BuildVerifyPackCmd(); }).Wait();
    }

    private string PackFlags => string.Format($"--no-color{(_quiet ? " --quiet" : "")}{(_timeStamps ? " --timestamps" : "")}{(_verbose ? " --verbose" : "")}");

    /// <summary>
    ///   Show current 'pack' version
    /// </summary>
    /// <returns>version</returns>
    public string Version()
    {
      var outputCapture = new OutputCapture();
      var processSpec = new ProcessSpec
      {
        EscapedArguments = PackFlags + " version",
        CancelOutputCapture = _cancellationToken,
        Executable = _packPath,
        OutputCapture = outputCapture
      };

      processSpec.Run(_log, _cancellationToken);

      var lines = string.Join(" ", outputCapture.Lines);

      return lines;
    }

    /// <summary>
    ///   Generate app image from source code
    /// </summary>
    /// <param name="imageName">Name of the new image</param>
    /// <param name="builder">Builder image</param>
    /// <param name="path">Artifact folder</param>
    /// <param name="buildSpec">Optional values to use while building</param>
    /// <param name="dryRun">Output complete command but don't run process</param>
    /// <exception cref="OperationCanceledException">The operation was cancelled by token</exception>
    /// <exception cref="ProcessException">The process returned a non-zero result (check command output for more info)</exception>
    /// <returns>The line-by-line output of the command</returns>
    /// <remarks>
    ///   Pack Build uses Cloud Native Buildpacks to create a runnable app image from source code.
    ///   Pack Build requires an image name, which will be generated from the source code. Build defaults to the current
    ///   directory, but you can use 'path' to specify another source code directory.
    ///   Build requires a builder, which can either be provided directly to build using 'builder'. For more on how to use pack
    ///   build, see: https://buildpacks.io/docs/app-developer-guide/build-an-app/.
    /// </remarks>
    public IEnumerable<string> Build([NotNull] string imageName, [NotNull] string builder, [NotNull] string path, PackBuildSpec buildSpec = default, bool dryRun = false)
    {
      // Validate required arguments
      Ensure.NotNullOrEmpty(imageName, nameof(imageName));
      Ensure.NotNullOrEmpty(builder, nameof(builder));
      Ensure.NotNullOrEmpty(path, nameof(path));

      // Validate argument combinations
      if (buildSpec?.Gid is < 0) throw new ArgumentException("gid value must be a positive integer");
      if (!string.IsNullOrEmpty(buildSpec?.CacheImage) && buildSpec?.Publish == false) throw new ArgumentException("To 'cacheImage' you must set 'publish' to true ");
      if (buildSpec?.Descriptor is {Exists: false}) throw new FileNotFoundException($"Description file could not be found at '{buildSpec.Descriptor.FullName}'");
      if (buildSpec?.EnvFile is {Exists: false}) throw new FileNotFoundException($"EnvFile file could not be found at '{buildSpec.EnvFile.FullName}'");

      // Initialize values
      var outputCapture = new OutputCapture();

      // Build command arguments
      var argumentsList = new List<string>
      {
        "build",
        $"\"{imageName}\"",
        $"--builder \"{builder}\"",
        $"--path \"{path}\""
      };

      if (buildSpec != null)
        argumentsList.AddRange(buildSpec.ToArgumentList());

      var arguments = string.Join(" ", argumentsList);

      var processEnvVariables = new Dictionary<string, string>
      {
        {"PACK_HOME", Path.Combine(path, "packHome")}
      };

      var processSpec = new ProcessSpec
      {
        EscapedArguments = arguments,
        EnvironmentVariables = processEnvVariables,
        CancelOutputCapture = _cancellationToken,
        Executable = _packPath,
        WorkingDirectory = Environment.CurrentDirectory,
        OutputCapture = outputCapture
      };

      if (dryRun)
        return new[] {string.Format($"{processSpec.Executable} {processSpec.EscapedArguments}")};

      try
      {
        processSpec.Run(_log, _cancellationToken);
      }
      catch (OperationCanceledException)
      {
        throw;
      }
      catch (ProcessException)
      {
        throw;
      }
      catch (Exception ex)
      {
        _log.LogError(ex, "An exception occurred while processing command");
        throw;
      }

      return outputCapture.Lines;
    }

    /// <summary>
    ///   Get information about a built app image
    /// </summary>
    /// <param name="imageName">The image name</param>
    /// <returns>the built app info</returns>
    public ImageInspection Inspect([NotNull] string imageName)
    {
      // Validate required arguments
      Ensure.NotNullOrEmpty(imageName, nameof(imageName));

      var outputCapture = new OutputCapture();
      var processSpec = new ProcessSpec
      {
        EscapedArguments = $"inspect \"{imageName}\" -o json",
        CancelOutputCapture = _cancellationToken,
        Executable = _packPath,
        WorkingDirectory = Environment.CurrentDirectory,
        OutputCapture = outputCapture
      };

      try
      {
        processSpec.Run(_log, _cancellationToken);
      }
      catch (OperationCanceledException)
      {
        throw;
      }
      catch (ProcessException)
      {
        throw;
      }
      catch (Exception ex)
      {
        _log.LogError(ex, "An exception occurred while processing command");
        throw;
      }

      //Convert the output to a safe json string
      var json = string.Join("", outputCapture.Lines);

      return ImageInspection.FromJson(json);
    }

    /// <summary>
    ///   Get bill of materials about a built app image
    /// </summary>
    /// <param name="imageName">The image name</param>
    /// <returns>the built app bill of materials</returns>
    public BillOfMaterials InspectBillOfMaterials(string imageName)
    {
      // Validate required arguments
      Ensure.NotNullOrEmpty(imageName, nameof(imageName));

      var outputCapture = new OutputCapture();
      var processSpec = new ProcessSpec
      {
        EscapedArguments = $"inspect \"{imageName}\" --bom -o json",
        CancelOutputCapture = _cancellationToken,
        Executable = _packPath,
        WorkingDirectory = Environment.CurrentDirectory,
        OutputCapture = outputCapture
      };

      try
      {
        processSpec.Run(_log, _cancellationToken);
      }
      catch (OperationCanceledException)
      {
        throw;
      }
      catch (ProcessException)
      {
        throw;
      }
      catch (Exception ex)
      {
        _log.LogError(ex, "An exception occurred while processing command");
        throw;
      }

      //Convert the output to a safe json string
      var json = string.Join("", outputCapture.Lines);

      return BillOfMaterials.FromJson(json);
    }

    private async Task<string> BuildVerifyPackCmd()
    {
      var resourceManifestName = "pack_csharp.Assets.pack_v0._19._0_linux.pack";
      var cmdPath = Path.Combine(Environment.CurrentDirectory, "pack");

      switch (Environment.OSVersion.Platform)
      {
        case PlatformID.Win32NT:
          resourceManifestName = "pack_csharp.Assets.pack_v0._19._0_windows.pack.exe";
          cmdPath = Path.Combine(Environment.CurrentDirectory, "pack.exe");
          break;
        case PlatformID.MacOSX:
          resourceManifestName = "pack_csharp.Assets.pack_v0._19._0_macos.pack";
          cmdPath = Path.Combine(Environment.CurrentDirectory, "pack");
          break;
        default:
          throw new Exception($"The OS '{Environment.OSVersion.Platform.ToString()}' is not supported by this library");
      }

      try
      {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames().First(str => str.EndsWith(resourceManifestName));

        if (resourceName is null)
          throw new Exception($"Could not find a resource named '{resourceManifestName}'");

        await using var file = File.OpenWrite(cmdPath);
        await using var stream = assembly.GetManifestResourceStream(resourceName);

        if (stream is null)
          throw new Exception("Could not find resource stream");

        await stream.CopyToAsync(file, _cancellationToken);
        await file.FlushAsync(_cancellationToken);
        await file.DisposeAsync();
      }
      catch (UnauthorizedAccessException ua)
      {
        _log.LogError(ua, $"Trying to write '{resourceManifestName}' to '{cmdPath}'");
        throw new Exception("This process does not have permission to save the pack binary");
      }
      catch (Exception ex)
      {
        _log.LogError(ex, "Error occurred trying to save pack binary");
        throw new Exception($"Error occurred trying to save pack binary, {ex.Message}");
      }

      if (!File.Exists(cmdPath))
        throw new FileNotFoundException($"Pack binary could not be validated at path '{cmdPath}'");

      return cmdPath;
    }
  }
}