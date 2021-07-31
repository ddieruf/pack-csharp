using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace pack_csharp_tests.Util
{
  public static class Artifact
  {
    public static async Task<string> Create(CancellationToken cancellationToken)
    {
      var srcPath = await GetApp(cancellationToken);
      var publishPath = PublishApp(srcPath);
      var artifactPath = CreateArtifact(publishPath);

      return artifactPath;
    }

    private static async Task<string> GetApp(CancellationToken cancellationToken)
    {
      var projectPath = Path.Combine(Environment.CurrentDirectory, "artifact-src");
      var projectName = "MyProject";

      if (Directory.Exists(Path.Combine(projectPath, projectName)) && Directory.GetFiles(Path.Combine(projectPath, projectName)).Any()) return Path.Combine(projectPath, projectName);

      if (Directory.Exists(projectPath))
        Directory.Delete(projectPath, true);

      var httpClient = new HttpClient
      {
        BaseAddress = new Uri("https://start.steeltoe.io/api/")
      };
      var bytes = await httpClient.GetByteArrayAsync($"project?Name={projectName}&Dependencies=management-endpoints", cancellationToken);

      await using var file = File.OpenWrite(Path.Combine(Environment.CurrentDirectory, "temp.zip"));
      await file.WriteAsync(bytes.AsMemory(0, bytes.Length), cancellationToken);
      await file.FlushAsync(cancellationToken);
      await file.DisposeAsync();

      using (var archive = new ZipArchive(File.OpenRead(Path.Combine(Environment.CurrentDirectory, "temp.zip"))))
      {
        archive.ExtractToDirectory(projectPath, true);
      }

      File.Delete(Path.Combine(Environment.CurrentDirectory, "temp.zip"));

      return Path.Combine(projectPath, projectName);
    }

    private static string PublishApp(string projectFolderPath)
    {
      var publishPath = Path.Combine(Environment.CurrentDirectory, "publish");

      if (Directory.Exists(publishPath))
        Directory.Delete(publishPath, true);

      using var process = new Process
      {
        EnableRaisingEvents = true,
        StartInfo = new ProcessStartInfo("dotnet")
        {
          Arguments = $"publish -o \"{publishPath}\" \"{projectFolderPath}\""
        }
      };

      try
      {
        process.Start();
      }
      finally
      {
        process.WaitForExit();
      }

      if (process.ExitCode != 0) throw new Exception($"Publish app process returned exit code {process.ExitCode}");

      var dirInfo = new DirectoryInfo(projectFolderPath);
      dirInfo.Parent?.Delete(true);

      return publishPath;
    }

    private static string CreateArtifact(string publishFolderPath)
    {
      var artifactPath = Path.Combine(Environment.CurrentDirectory, "artifact.zip");

      if (File.Exists(artifactPath))
        File.Delete(artifactPath);

      ZipFile.CreateFromDirectory(publishFolderPath, artifactPath);

      Directory.Delete(publishFolderPath, true);

      return artifactPath;
    }
  }
}