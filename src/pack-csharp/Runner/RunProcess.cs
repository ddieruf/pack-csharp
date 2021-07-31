using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace pack_csharp.Runner
{
  public static class ProcessSpecExtensions
  {
    public static void Run(this ProcessSpec processSpec, ILogger logger, CancellationToken cancellationToken)
    {
      if (logger is null)
        throw new ArgumentNullException(nameof(logger));

      if (processSpec is null)
        throw new ArgumentNullException(nameof(processSpec));

      var processRunner = new ProcessRunner(logger);
      var cancelledTaskSource = new TaskCompletionSource<int>();
      cancellationToken.Register(state => ((TaskCompletionSource<int>) state).TrySetCanceled(), cancelledTaskSource);

      var processTask = processRunner.RunAsync(processSpec, cancellationToken);

      //The process ran to completion and the result was not success
      if (processTask.Result != 0 && !cancellationToken.IsCancellationRequested)
        throw new ProcessException(string.Join(Environment.NewLine, processSpec.OutputCapture?.Lines ?? new List<string> {$"Process exited with result '{processTask.Result}'"}));

      //The process was cancelled
      if (cancellationToken.IsCancellationRequested)
      {
        logger.LogDebug("Process was cancelled");
        throw new OperationCanceledException();
      }

      //The process ran to completion and the result was success
      logger.LogDebug("Process completed successfully");
    }
  }
}