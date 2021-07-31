using System.Threading;
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

    [Fact(DisplayName = "Get image inspection success")]
    public void GetImageInspection()
    {
      var logger = _outputHelper.ToLogger<Pack>();
      var cts = new CancellationTokenSource();

      var pack = new Pack(cts.Token, logger);
      var bom = pack.InspectBillOfMaterials("my-image");

      bom.Should().NotBeNull();
      bom.Local.Should().Contain(q => q.Name == "dotnet-runtime");
      bom.Local.Should().Contain(q => q.Name == "dotnet-aspnetcore");
    }
  }
}
