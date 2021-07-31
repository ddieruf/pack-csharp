namespace pack_csharp.Runner
{
  public class OutputSink
  {
    public OutputCapture Current { get; private set; }

    public OutputCapture StartCapture()
    {
      return Current = new OutputCapture();
    }
  }
}