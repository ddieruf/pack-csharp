using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace pack_csharp.Runner
{
  public class ProcessRunner
  {
    private readonly ILogger logger;

    public ProcessRunner(ILogger logger)
    {
      if (logger is null)
        throw new ArgumentNullException(nameof(logger));

      this.logger = logger;
    }

    public async Task<int> RunAsync(ProcessSpec processSpec, CancellationToken cancellationToken)
    {
      if (processSpec is null)
        throw new ArgumentNullException(nameof(processSpec));

      int exitCode;

      var stopwatch = new Stopwatch();

      using (var process = CreateProcess(processSpec))
      using (var processState = new ProcessState(process, logger))
      {
        cancellationToken.Register(() => processState.TryKill());

        var readOutput = false;
        var readError = false;
        if (processSpec.IsOutputCaptured)
        {
          readOutput = true;
          readError = true;
          process.OutputDataReceived += (_, a) =>
          {
            if (!string.IsNullOrEmpty(a.Data)) processSpec.OutputCapture.AddLine(a.Data);
          };
          process.ErrorDataReceived += (_, a) =>
          {
            if (!string.IsNullOrEmpty(a.Data)) processSpec.OutputCapture.AddLine(a.Data);
          };
        }
        else if (processSpec.OnOutput != null)
        {
          readOutput = true;
          process.OutputDataReceived += processSpec.OnOutput;
        }

        stopwatch.Start();
        process.Start();

        logger.LogDebug($"Started \"{processSpec.Executable} {process.StartInfo.Arguments}\" with process id {process.Id}");

        if (readOutput) process.BeginOutputReadLine();
        if (readError) process.BeginErrorReadLine();

        await processState.Task;

        exitCode = process.ExitCode;
        stopwatch.Stop();
        logger.LogDebug($"Process id {process.Id} ran for {stopwatch.ElapsedMilliseconds}ms and existed with code {exitCode}");
      }

      return exitCode;
    }

    private static Process CreateProcess(ProcessSpec processSpec)
    {
      var process = new Process
      {
        EnableRaisingEvents = true,
        StartInfo =
        {
          FileName = processSpec.Executable,
          Arguments = processSpec.EscapedArguments ?? ArgumentEscaper.EscapeAndConcatenateArgArrayForProcessStart(processSpec.Arguments),
          UseShellExecute = false,
          WorkingDirectory = processSpec.WorkingDirectory,
          RedirectStandardOutput = processSpec.IsOutputCaptured || processSpec.OnOutput != null,
          RedirectStandardError = processSpec.IsOutputCaptured
        }
      };

      foreach (var env in processSpec.EnvironmentVariables) process.StartInfo.Environment.Add(env.Key, env.Value);

      return process;
    }

    private void SetEnvironmentVariable(ProcessStartInfo processStartInfo, string envVarName, List<string> envVarValues, char separator)
    {
      if (envVarValues is {Count: 0}) return;

      var existing = Environment.GetEnvironmentVariable(envVarName);

      string result;
      if (!string.IsNullOrEmpty(existing))
        result = existing + separator + string.Join(separator, envVarValues);
      else
        result = string.Join(separator, envVarValues);

      processStartInfo.EnvironmentVariables[envVarName] = result;
    }

    private class ProcessState : IDisposable
    {
      private readonly Process _process;
      private readonly TaskCompletionSource<object> _tcs = new();
      private readonly ILogger logger;
      private volatile bool _disposed;

      public ProcessState(Process process, ILogger logger)
      {
        this.logger = logger;
        _process = process;
        _process.Exited += OnExited;
        Task = _tcs.Task.ContinueWith(_ =>
        {
          try
          {
            // We need to use two WaitForExit calls to ensure that all of the output/events are processed. Previously
            // this code used Process.Exited, which could result in us missing some output due to the ordering of
            // events.
            //
            // See the remarks here: https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.process.waitforexit#System_Diagnostics_Process_WaitForExit_System_Int32_
            if (!_process.WaitForExit(int.MaxValue)) throw new TimeoutException();

            _process.WaitForExit();
          }
          catch (InvalidOperationException)
          {
            // suppress if this throws if no process is associated with this object anymore.
          }
        });
      }

      public Task Task { get; }

      public void Dispose()
      {
        if (!_disposed)
        {
          TryKill();
          _disposed = true;
          _process.Exited -= OnExited;
          _process.Dispose();
        }
      }

      public void TryKill()
      {
        if (_disposed) return;

        try
        {
          if (_process is not null && !_process.HasExited)
          {
            logger.LogDebug($"Killing process {_process.Id}");
            _process.Kill();
          }
        }
        catch (Exception ex)
        {
          logger.LogDebug($"Error while killing process '{_process.StartInfo.FileName} {_process.StartInfo.Arguments}': {ex.Message}");
#if DEBUG
          logger.LogDebug(ex.ToString());
#endif
        }
      }

      private void OnExited(object sender, EventArgs args)
      {
        _tcs.TrySetResult(null);
      }
    }
  }
}