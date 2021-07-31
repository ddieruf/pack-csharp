using System.Collections.Generic;

namespace pack_csharp.Runner
{
  public class OutputCapture
  {
    private readonly List<string> _lines = new();
    public IEnumerable<string> Lines => _lines;

    public void AddLine(string line)
    {
      _lines.Add(line);
    }
  }
}