using System.Threading;
using FluentAssertions;
using pack_csharp;
using Xunit;
using Xunit.Abstractions;

namespace pack_csharp_tests
{
  public class Version
  {
    private readonly ITestOutputHelper _outputHelper;

    public Version(ITestOutputHelper outputHelper)
    {
      _outputHelper = outputHelper;
    }

    [Fact(DisplayName = "Get version success")]
    public void GetVersion()
    {
      var logger = _outputHelper.ToLogger<Pack>();
      var cts = new CancellationTokenSource();

      var pack = new Pack(cts.Token, logger);
      var version = pack.Version();
      version.Should().Be("0.19.0+git-360dbae.build-2550");
    }
  }
}
