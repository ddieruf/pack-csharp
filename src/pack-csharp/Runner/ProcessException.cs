using System;

namespace pack_csharp.Runner
{
  public class ProcessException : Exception
  {
    public ProcessException()
    {
    }

    public ProcessException(string message) : base(message)
    {
    }
  }
}