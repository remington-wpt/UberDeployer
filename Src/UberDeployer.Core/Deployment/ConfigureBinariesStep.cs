﻿using System;
using System.Diagnostics;
using System.IO;

namespace UberDeployer.Core.Deployment
{
  public class ConfigureBinariesStep : DeploymentStep
  {
    private const int _ChildProcessTimeoutInMilliseconds = 60 * 1000;

    private readonly string _templateConfigurationName;
    private readonly string _artifactsDirPath;

    public ConfigureBinariesStep(string templateConfigurationName, string artifactsDirPath)
    {
      if (string.IsNullOrWhiteSpace(artifactsDirPath))
      {
        throw new ArgumentNullException("artifactsDirPath");
      }

      if (string.IsNullOrWhiteSpace(templateConfigurationName))
      {
        throw new ArgumentNullException("templateConfigurationName");
      }

      _templateConfigurationName = templateConfigurationName;
      _artifactsDirPath = artifactsDirPath;
    }

    protected override void DoExecute()
    {
      Execute(
        Path.Combine(
          _artifactsDirPath,
          string.Format("Config_{0}.bat", _templateConfigurationName)),
        _artifactsDirPath,
        "--no-pause");
    }

    public override string Description
    {
      get { return string.Format("Run Config_{0}.bat in order to create environment-specific artifacts.", _templateConfigurationName); }
    }

    private void Execute(string fileToExecute, string workingDir, string arguments)
    {
      var processStartInfo =
        new ProcessStartInfo
        {
          FileName = fileToExecute,
          WorkingDirectory = workingDir,
          CreateNoWindow = true,
          UseShellExecute = false,
          WindowStyle = ProcessWindowStyle.Hidden,
          RedirectStandardError = true,
          RedirectStandardOutput = true,
          Arguments = arguments,
        };

      var stopwatch = new Stopwatch();

      stopwatch.Start();

      bool errorOccured = false;

      try
      {
        using (Process exeProcess = Process.Start(processStartInfo))
        {
          exeProcess.EnableRaisingEvents = true;
          exeProcess.OutputDataReceived += (sender, args) =>
          {
            if (string.IsNullOrEmpty(args.Data) == false)
            {
              PostDiagnosticMessage(args.Data, DiagnosticMessageType.Trace);
            }
          };

          exeProcess.ErrorDataReceived += (sender, args) =>
          {
            if (string.IsNullOrEmpty(args.Data) == false)
            {
              PostDiagnosticMessage(args.Data, DiagnosticMessageType.Error);
              errorOccured = true;
            }
          };

          exeProcess.BeginErrorReadLine();
          exeProcess.BeginOutputReadLine();

          bool finished = exeProcess.WaitForExit(_ChildProcessTimeoutInMilliseconds);

          if (!finished)
          {
            throw new InternalException(string.Format("Timed out while waiting for the child process to finish (waited for '{0}' seconds).", (_ChildProcessTimeoutInMilliseconds / 1000)));
          }

          if (exeProcess.ExitCode > 0 || errorOccured)
          {
            throw new InternalException("Error on executing command line.");
          }
        }
      }
      finally
      {
        stopwatch.Stop();

        PostDiagnosticMessage(string.Format("Executing file [{0}] took: {1} s.", fileToExecute, stopwatch.Elapsed.TotalSeconds), DiagnosticMessageType.Info);
      }
    }
  }
}
