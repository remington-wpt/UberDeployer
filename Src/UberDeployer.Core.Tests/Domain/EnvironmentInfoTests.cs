﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using UberDeployer.Core.Domain;

namespace UberDeployer.Core.Tests.Domain
{
  // TODO IMM HI: formatting
  [TestFixture]
  public class EnvironmentInfoTests
  {
    private const string _AppServerMachine = "app_server";
    private const string _FailoverClusterMachineName = "failover_cluster";
    private const string _EnvironmentName = "environment";
    private const string _ConfigurationTemplateName = "conf";
    private static readonly List<string> _WebMachineNames = new List<string> { "web1", "web2" };
    private const string _TerminalMachineName = "terminal_server";
    private static readonly List<string> _SchedulerServerTasksMachineNames = new List<string> { "scheduler_tasks_server", };
    private static readonly List<string> _SchedulerServerBinariesMachineNames = new List<string> { "scheduler_binaries_server", };
    private const string _NtServicesBaseDirPath = "C:\\NtServices";
    private const string _WebAppsBaseDirPath = "C:\\WebApps";
    private const string _SchedulerAppsBaseDirPath = "C:\\SchedulerApps";
    private const string _TerminalAppsBaseDirPath = "C:\\TerminalApps";
    private const string _TerminalAppsShortcutFolder = "C:\\TerminalAppShortcuts";
    private const string _ArtifactsDeploymentDirPath = "C:\\ArtifactsDeploymentDirPath";

    [Test]
    public void Test_EnvironmentInfoTests_Does_Not_Allow_Template_null()
    {
      Assert.Throws<ArgumentNullException>(
        () =>
        {
          new EnvironmentInfo(
            _EnvironmentName,
            null,
            _AppServerMachine,
            _FailoverClusterMachineName,
            _WebMachineNames,
            _TerminalMachineName,
            _SchedulerServerTasksMachineNames,
            _SchedulerServerBinariesMachineNames,
            _NtServicesBaseDirPath,
            _WebAppsBaseDirPath,
            _SchedulerAppsBaseDirPath,
            _TerminalAppsBaseDirPath,
            false,
            TestData.EnvironmentUsers,
            TestData.AppPoolInfos,
            TestData.DatabaseServers,
            TestData.ProjectToFailoverClusterGroupMappings,
            TestData.WebAppProjectConfigurationOverrides,
            TestData.DbProjectConfigurationOverrides,
            _TerminalAppsShortcutFolder,
            _ArtifactsDeploymentDirPath);
        });
    }

    [Test]
    public void Test_EnvironmentInfoTests_Allows_Empty_Template()
    {
      Assert.DoesNotThrow(
        () =>
        {
          new EnvironmentInfo(
            _EnvironmentName,
            "",
            _AppServerMachine,
            _FailoverClusterMachineName,
            _WebMachineNames,
            _TerminalMachineName,
            _SchedulerServerTasksMachineNames,
            _SchedulerServerBinariesMachineNames,
            _NtServicesBaseDirPath,
            _WebAppsBaseDirPath,
            _SchedulerAppsBaseDirPath,
            _TerminalAppsBaseDirPath,
            false,
            TestData.EnvironmentUsers,
            TestData.AppPoolInfos,
            TestData.DatabaseServers,
            TestData.ProjectToFailoverClusterGroupMappings,
            TestData.WebAppProjectConfigurationOverrides,
            TestData.DbProjectConfigurationOverrides,
            _TerminalAppsShortcutFolder,
            _ArtifactsDeploymentDirPath);
        });
    }

    [Test]
    public void Test_GetAppServerNetworkPath_Throws_When_path_startswithbackslashes()
    {
      var envInfo = new EnvironmentInfo(
        _EnvironmentName,
        _ConfigurationTemplateName,
        _AppServerMachine,
        _FailoverClusterMachineName,
        _WebMachineNames,
        _TerminalMachineName,
        _SchedulerServerTasksMachineNames,
        _SchedulerServerBinariesMachineNames,
        _NtServicesBaseDirPath,
        _WebAppsBaseDirPath,
        _SchedulerAppsBaseDirPath,
        _TerminalAppsBaseDirPath,
        false,
        TestData.EnvironmentUsers,
        TestData.AppPoolInfos,
        TestData.DatabaseServers,
        TestData.ProjectToFailoverClusterGroupMappings,
        TestData.WebAppProjectConfigurationOverrides,
        TestData.DbProjectConfigurationOverrides,
        _TerminalAppsShortcutFolder,
        _ArtifactsDeploymentDirPath);

      Assert.Throws<ArgumentException>(
        () => envInfo.GetAppServerNetworkPath(@"\\kasjdkasdj"));
    }

    [Test]
    public void Test_GetAppServerNetworkPath_Throws_When_path_doesntstartwithdriveletter()
    {
      var envInfo =
        new EnvironmentInfo(
          _EnvironmentName,
          _ConfigurationTemplateName,
          _AppServerMachine,
          _FailoverClusterMachineName,
          _WebMachineNames,
          _TerminalMachineName,
          _SchedulerServerTasksMachineNames,
          _SchedulerServerBinariesMachineNames,
          _NtServicesBaseDirPath,
          _WebAppsBaseDirPath,
          _SchedulerAppsBaseDirPath,
          _TerminalAppsBaseDirPath,
          false,
          TestData.EnvironmentUsers,
          TestData.AppPoolInfos,
          TestData.DatabaseServers,
          TestData.ProjectToFailoverClusterGroupMappings,
          TestData.WebAppProjectConfigurationOverrides,
          TestData.DbProjectConfigurationOverrides,
          _TerminalAppsShortcutFolder,
          _ArtifactsDeploymentDirPath);

      Assert.Throws<ArgumentException>(
        () => envInfo.GetAppServerNetworkPath("qlwelqwelw"));
    }

    [Test]
    public void GetWebServerNetworkPath_WhenAbsoluteLocalPathIsCorrect_ReturnCorrectPath()
    {
      var envInfo =
        new EnvironmentInfo(
          _EnvironmentName,
          _ConfigurationTemplateName,
          _AppServerMachine,
          _FailoverClusterMachineName,
          _WebMachineNames,
          _TerminalMachineName,
          _SchedulerServerTasksMachineNames,
          _SchedulerServerBinariesMachineNames,
          _NtServicesBaseDirPath,
          _WebAppsBaseDirPath,
          _SchedulerAppsBaseDirPath,
          _TerminalAppsBaseDirPath,
          false,
          TestData.EnvironmentUsers,
          TestData.AppPoolInfos,
          TestData.DatabaseServers,
          TestData.ProjectToFailoverClusterGroupMappings,
          TestData.WebAppProjectConfigurationOverrides,
          TestData.DbProjectConfigurationOverrides,
          _TerminalAppsShortcutFolder,
          _ArtifactsDeploymentDirPath);

      Assert.AreEqual(
        "\\\\" + _WebMachineNames[0] + "\\c$\\",
        envInfo.GetWebServerNetworkPath(_WebMachineNames[0], "c:\\"));
    }
  }
}
