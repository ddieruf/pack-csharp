using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using pack_csharp;
using pack_csharp.Util;
using pack_csharp_tests.Util;
using Xunit;
using Xunit.Abstractions;

namespace pack_csharp_tests
{
  public class Build
  {
    private readonly CancellationTokenSource _cts = new();
    private readonly ILoggerFactory _loggerFactory;
    private string _artifactPath;

    public Build(ITestOutputHelper outputHelper)
    {
      _loggerFactory = new LoggerFactory().AddXUnit(outputHelper, LogLevel.Trace);

      Task.Run(async () => { _artifactPath = await Artifact.Create(_cts.Token); }).Wait();
    }

    public static IEnumerable<object[]> BuildSenarios()
    {
      var imageName = "some-repo/my-image";
      var builder = "paketobuildpacks/builder:base";

      // No image name
      yield return new object[]
      {
        string.Empty,
        builder,
        new PackBuildSpec(),
        typeof(ArgumentException)
      };

      // No builder name
      yield return new object[]
      {
        imageName,
        string.Empty,
        new PackBuildSpec(),
        typeof(ArgumentException)
      };

      // Minimum values
      yield return new object[]
      {
        imageName,
        builder,
        null,
        null
      };
      // Minimum values
      yield return new object[]
      {
        imageName,
        builder,
        new PackBuildSpec(Env: new SortedList<string, string> {{"ASPNET_ENVIRONMENT", "Staging"}}, Tags: new SortedSet<string> {"latest", "1234"}),
        null
      };
    }

    [Theory(DisplayName = "Build senarios")]
    [MemberData(nameof(BuildSenarios))]
    public async Task BuildValSenarios(string imageName, string builder, PackBuildSpec buildSpec, Type exceptionType)
    {
      var logger = _loggerFactory.CreateLogger<Pack>();
      var cts = new CancellationTokenSource();

      var pack = new Pack(logger);

      try
      {
        var output = await pack.Build(imageName, builder, _artifactPath, buildSpec, cancellationToken: cts.Token);
        logger.LogDebug(string.Join(Environment.NewLine, output));
      }
      catch (Exception ex)
      {
        if (exceptionType == null) throw;
        Assert.IsType(exceptionType, ex);
      }
    }
  }
}
