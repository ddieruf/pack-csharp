using System.Threading;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using pack_csharp;
using Xunit;
using Xunit.Abstractions;

namespace pack_csharp_tests
{
  public class Inspect
  {
    private readonly ITestOutputHelper _outputHelper;

    public Inspect(ITestOutputHelper outputHelper)
    {
      _outputHelper = outputHelper;
    }

    [Fact(DisplayName = "Get image inspection success")]
    public void GetImageInspection()
    {
      var logger = _outputHelper.ToLogger<Pack>();
      var cts = new CancellationTokenSource();

      var pack = new Pack(cts.Token, logger);
      var inspection = pack.Inspect("my-image");

      inspection.Should().NotBeNull();
      inspection.ImageName.Should().Be("my-image");
    }
  }
}
