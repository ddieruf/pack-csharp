using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
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
    public async Task GetImageInspection()
    {
      var logger = _outputHelper.ToLogger<Pack>();
      var cts = new CancellationTokenSource();

      var pack = new Pack(logger);
      var inspection = await pack.Inspect("my-image", cts.Token);

      inspection.Should().NotBeNull();
      inspection.ImageName.Should().Be("my-image");
    }
  }
}
