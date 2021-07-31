using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Kpush.Sdks.Runner
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
			var cancelledTaskSource = new TaskCompletionSource();
			cancellationToken.Register(state => ((TaskCompletionSource) state).TrySetResult(), cancelledTaskSource);

			var processTask = processRunner.RunAsync(processSpec, cancellationToken);
			
			//The process ran to completion and the result was not success
			if (processTask.Result != 0 && !cancellationToken.IsCancellationRequested)
			{
				throw new Exception("Process exited with error");
			}

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