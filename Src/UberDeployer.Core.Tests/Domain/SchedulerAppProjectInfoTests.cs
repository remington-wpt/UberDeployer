﻿using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using UberDeployer.Common.IO;
using UberDeployer.Core.Deployment;
using UberDeployer.Core.Domain;
using UberDeployer.Core.Management.ScheduledTasks;

namespace UberDeployer.Core.Tests.Domain
{
  [TestFixture]
  public class SchedulerAppProjectInfoTests
  {
    private const string _ProjectName = "name";
    private const string _ArtifactsRepositoryName = "repoName";
    private const string _ArtifactsRepositoryDirName = "repoDirName";
    private const bool _ArtifactsAreNotEnvirionmentSpecific = false;
    private const string _SchedulerAppName = "appName";
    private const string _SchedulerAppDirName = "appDirName";
    private const string _SchedulerAppExeName = "appExeName";
    private const string _SchedulerAppUserId = "appUser";
    private const int _ScheduledHour = 1;
    private const int _ScheduledMinute = 1;
    private const int _ExecutionTimeLimitInMinutes = 1;
    private static readonly Repetition _Repetition = Repetition.CreateEnabled(TimeSpan.FromMinutes(15.0), TimeSpan.FromDays(1.0), true);
    private static readonly string[] _AllowedEnvironmentNames = { "env_name" };

    private Mock<IObjectFactory> _objectFactoryFake;

    [SetUp]
    public void SetUp()
    {
      _objectFactoryFake = new Mock<IObjectFactory>(MockBehavior.Loose);
    }

    [Test]
    public void Test_CreateDeployemntTask_RunsProperly_WhenAllIsWell()
    {
      var objectFactory = new Mock<IObjectFactory>(MockBehavior.Strict);
      var prjInfoRepository = new Mock<IProjectInfoRepository>(MockBehavior.Loose);
      var envInfoRepository = new Mock<IEnvironmentInfoRepository>(MockBehavior.Loose);
      var artifactsRepository = new Mock<IArtifactsRepository>(MockBehavior.Loose);
      var taskScheduler = new Mock<ITaskScheduler>(MockBehavior.Loose);
      var passwordCollector = new Mock<IPasswordCollector>(MockBehavior.Loose);
      var directoryAdapter = new Mock<IDirectoryAdapter>(MockBehavior.Loose);
      var fileAdapter = new Mock<IFileAdapter>(MockBehavior.Loose);
      var zipFileAdapter = new Mock<IZipFileAdapter>(MockBehavior.Loose);

      var schedulerAppProjectInfo =
        new SchedulerAppProjectInfo(
          _ProjectName,
          _ArtifactsRepositoryName,
          _AllowedEnvironmentNames,
          _ArtifactsRepositoryDirName,
          _ArtifactsAreNotEnvirionmentSpecific,
          _SchedulerAppDirName,
          _SchedulerAppExeName,
          new List<SchedulerAppTask>
          {
            new SchedulerAppTask(
              _SchedulerAppName,
              _SchedulerAppName,
              _SchedulerAppUserId,
              _ScheduledHour,
              _ScheduledMinute,
              _ExecutionTimeLimitInMinutes,
              _Repetition)
          });

      objectFactory.Setup(o => o.CreateProjectInfoRepository()).Returns(prjInfoRepository.Object);
      objectFactory.Setup(o => o.CreateEnvironmentInfoRepository()).Returns(envInfoRepository.Object);
      objectFactory.Setup(o => o.CreateArtifactsRepository()).Returns(artifactsRepository.Object);
      objectFactory.Setup(o => o.CreateTaskScheduler()).Returns(taskScheduler.Object);
      objectFactory.Setup(o => o.CreatePasswordCollector()).Returns(passwordCollector.Object);
      objectFactory.Setup(o => o.CreateDirectoryAdapter()).Returns(directoryAdapter.Object);
      objectFactory.Setup(o => o.CreateFileAdapter()).Returns(fileAdapter.Object);
      objectFactory.Setup(o => o.CreateZipFileAdapter()).Returns(zipFileAdapter.Object);

      schedulerAppProjectInfo.CreateDeploymentTask(objectFactory.Object);
    }

    [Test]
    public void Test_GetTargetFolde_RunsProperly_WhenAllIsWell()
    {
      string machine = Environment.MachineName;
      const string baseDirPath = "c:\\basedir";

      var envInfo =
        new EnvironmentInfo(
          "name",
          "templates",
          "appservermachine",
          "failover",
          new[] { "webmachine" },
          "terminalmachine",
          new[] { machine },
          new[] { machine },
          baseDirPath,
          "webbasedir",
          "c:\\scheduler",
          "terminal",
          false,
          TestData.EnvironmentUsers,
          TestData.AppPoolInfos,
          TestData.DatabaseServers,
          TestData.ProjectToFailoverClusterGroupMappings,
          TestData.WebAppProjectConfigurationOverrides,
          TestData.DbProjectConfigurationOverrides,
          "terminalAppsShortcutFolder",
          "artifactsDeploymentDirPath");

      var schedulerAppProjectInfo =
        new SchedulerAppProjectInfo(
          _ProjectName,
          _ArtifactsRepositoryName,
          _AllowedEnvironmentNames,
          _ArtifactsRepositoryDirName,
          _ArtifactsAreNotEnvirionmentSpecific,
          _SchedulerAppDirName,
          _SchedulerAppExeName,
          new List<SchedulerAppTask>
          {
            new SchedulerAppTask(
              _SchedulerAppName,
              _SchedulerAppName,
              _SchedulerAppUserId,
              _ScheduledHour,
              _ScheduledMinute,
              _ExecutionTimeLimitInMinutes,
              _Repetition)
          });

      List<string> targetFolders =
        schedulerAppProjectInfo.GetTargetFolders(_objectFactoryFake.Object, envInfo)
          .ToList();

      Assert.IsNotNull(targetFolders);
      Assert.AreEqual(1, targetFolders.Count);
      Assert.AreEqual("\\\\" + machine + "\\c$\\scheduler\\" + _SchedulerAppDirName, targetFolders[0]);
    }
  }
}
