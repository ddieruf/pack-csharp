using System.Threading;
using FluentAssertions;
using pack_csharp;
using Xunit;
using Xunit.Abstractions;

namespace pack_csharp_tests
{
  public class Const
  {
    private readonly ITestOutputHelper _outputHelper;

    public Const(ITestOutputHelper outputHelper)
    {
      _outputHelper = outputHelper;
    }

    [Fact(DisplayName = "Create base constructor")]
    public void CreateConst()
    {
      var logger = _outputHelper.ToLogger<Pack>();
      var cts = new CancellationTokenSource();

      var pack = new Pack(cts.Token, logger);
      pack.Should().NotBeNull();
    }

    [Fact(DisplayName = "Create full constructor")]
    public void CreateConstFull()
    {
      var logger = _outputHelper.ToLogger<Pack>();
      var cts = new CancellationTokenSource();
      const bool quiet = false;
      const bool verbose = true;
      const bool timeStamps = true;

      var pack = new Pack(cts.Token, logger, quiet, verbose, timeStamps);
      pack.Should().NotBeNull();
    }
  }
}