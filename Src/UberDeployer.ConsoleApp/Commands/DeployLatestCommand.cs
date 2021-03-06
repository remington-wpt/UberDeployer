﻿using System;
using System.Linq;
using System.Security.Principal;
using UberDeployer.CommonConfiguration;
using UberDeployer.ConsoleCommander;
using UberDeployer.Core.Deployment;
using UberDeployer.Core.Deployment.Pipeline;
using UberDeployer.Core.Domain;
using UberDeployer.Core.TeamCity;
using UberDeployer.Core.TeamCity.Models;

namespace UberDeployer.ConsoleApp.Commands
{
  public class DeployLatestCommand : DeployCommand
  {
    public DeployLatestCommand(CommandDispatcher commandDispatcher)
      : base(commandDispatcher)
    {
    }

    public override string CommandName
    {
      get { return "deploy-latest"; }
    }

    public override int Run(string[] args)
    {
      if (args.Length != 3 && args.Length != 4)
      {
        DisplayCommandUsage();
        return 1;
      }

      IProjectInfoRepository projectInfoRepository =
        ObjectFactory.Instance.CreateProjectInfoRepository();

      string projectName = args[0];
      string projectConfigurationName = args[1];
      string targetEnvironmentName = args[2];
      bool isSimulation = (args.Length >= 4 ? string.Equals(args[3], "simulate", StringComparison.OrdinalIgnoreCase) : false);

      ProjectInfo projectInfo = projectInfoRepository.FindByName(projectName);

      if (projectInfo == null)
      {
        OutputWriter.WriteLine("Project named '{0}' doesn't exist.", projectName);
        
        return 1;
      }

      ITeamCityClient teamCityClient =
        ObjectFactory.Instance.CreateTeamCityClient();

      Project project = teamCityClient.GetProjectByName(projectInfo.ArtifactsRepositoryName);
      ProjectDetails projectDetails = teamCityClient.GetProjectDetails(project);

      ProjectConfiguration projectConfiguration =
        projectDetails.ConfigurationsList
          .Configurations
          .SingleOrDefault(pc => pc.Name == projectConfigurationName);

      if (projectConfiguration == null)
      {
        OutputWriter.WriteLine(
          "Project configuration named '{0}' doesn't exist for project '{1}'.",
          projectConfigurationName,
          projectName);

        return 0;
      }

      ProjectConfigurationDetails projectConfigurationDetails =
        teamCityClient.GetProjectConfigurationDetails(projectConfiguration);

      ProjectConfigurationBuild projectConfigurationBuild =
        teamCityClient.GetProjectConfigurationBuilds(projectConfigurationDetails, 0, 1)
          .Builds
          .FirstOrDefault();

      if (projectConfigurationBuild == null)
      {
        throw new InvalidOperationException(
          string.Format(
            "Project configuration '{0}' of project '{1}' doesn't have any builds yet.",
            projectConfigurationName,
            projectName));
      }

      if (projectConfigurationBuild.Status != BuildStatus.Success)
      {
        throw new InvalidOperationException(
          string.Format(
            "Couldn't deploy latest build of project configuration '{0}' of project '{1}' because it was not successfull.",
            projectConfigurationName,
            projectName));
      }

      string projectConfigurationBuildId = projectConfigurationBuild.Id;

      try
      {
        DeploymentTask deploymentTask =
          projectInfo.CreateDeploymentTask(ObjectFactory.Instance);

        Guid deploymentId = Guid.NewGuid();

        var deploymentInfo =
          new DeploymentInfo(
            deploymentId,
            isSimulation,
            projectName,
            projectConfigurationName,
            projectConfigurationBuildId,
            targetEnvironmentName,
            projectInfo.CreateEmptyInputParams());

        IDeploymentPipeline deploymentPipeline =
          ObjectFactory.Instance.CreateDeploymentPipeline();

        deploymentPipeline.DiagnosticMessagePosted +=
          (eventSender, tmpArgs) => LogMessage(tmpArgs.Message, tmpArgs.MessageType);

        var deploymentContext = new DeploymentContext(RequesterIdentity);

        deploymentPipeline.StartDeployment(deploymentInfo, deploymentTask, deploymentContext);

        return 0;
      }
      catch (Exception exc)
      {
        LogMessage("Error: " + exc, DiagnosticMessageType.Error);

        return 1;
      }
    }

    public override void DisplayCommandUsage()
    {
      OutputWriter.WriteLine("Usage: {0} project projectConfiguration targetEnvironment", CommandName);
    }

    private static string RequesterIdentity
    {
      get
      {
        var windowsIdentity = WindowsIdentity.GetCurrent();

        if (windowsIdentity == null)
        {
          throw new InternalException("Couldn't get requester identity.");
        }

        return windowsIdentity.Name;
      }
    }
  }
}
