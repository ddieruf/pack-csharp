using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using pack_csharp;
using Xunit;
using Xunit.Abstractions;

namespace pack_csharp_tests
{
  public class BillOfMaterials
  {
    private readonly ITestOutputHelper _outputHelper;

    public BillOfMaterials(ITestOutputHelper outputHelper)
    {
      _outputHelper = outputHelper;
    }

    [Fact(DisplayName = "Get BOM success")]
    public async Task GetImageInspection()
    {
      var logger = _outputHelper.ToLogger<Pack>();
      var cts = new CancellationTokenSource();

      var pack = new Pack(logger);
      var bom = await pack.InspectBillOfMaterials("my-image", cts.Token);

      bom.Should().NotBeNull();
      bom.Local.Should().Contain(q => q.Name == "dotnet-runtime");
      bom.Local.Should().Contain(q => q.Name == "dotnet-aspnetcore");
    }
  }
}
