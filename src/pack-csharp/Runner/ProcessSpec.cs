using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace pack_csharp.Runner
{
  public record ProcessSpec
  {
    public string Executable { get; init; }
    public string WorkingDirectory { get; init; }
    public IReadOnlyDictionary<string, string> EnvironmentVariables { get; init; } = new Dictionary<string, string>();

    public IReadOnlyList<string> Arguments { get; init; }
    public string EscapedArguments { get; init; }
    public OutputCapture OutputCapture { get; init; }

    public bool IsOutputCaptured => OutputCapture != null;

    public DataReceivedEventHandler OnOutput { get; init; }

    public CancellationToken CancelOutputCapture { get; init; }

    public string ShortDisplayName()
    {
      return Path.GetFileNameWithoutExtension(Executable);
    }
  }
}