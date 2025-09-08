using Octokit;
using PtzJoystickControl.Core.Model;
using PtzJoystickControl.Core.Services;
using System.Text.RegularExpressions;

namespace PtzJoystickControl.Application.Services;

public class UpdateService : IUpdateService
{
    private readonly IGitHubClient _githubClient;
    private readonly string _owner;
    private readonly string _repository;
    private readonly Version _currentVersion;

    public UpdateService(IGitHubClient githubClient, string owner, string repository, Version currentVersion)
    {
        _githubClient = githubClient;
        _owner = owner;
        _repository = repository;
        this._currentVersion = currentVersion;
    }

    public async Task<Update> CheckForUpdate()
    {
        Release latestRelease;
        try
        {
            latestRelease = await _githubClient.Repository.Release.GetLatest(_owner, _repository);
        }
        catch (Exception e)
        {
            return new Update
            {
                Error = true,
                ErrorMessage = e.Message
            };
        }

        var update = new Update()
        {
            Version = latestRelease.TagName,
            Uri = new Uri(latestRelease.HtmlUrl)
        };

        var versionMatch = Regex.Match(latestRelease.TagName, @"\d+\.\d+\.\d+(\.\d+)?");

        if (!versionMatch.Success || !Version.TryParse(versionMatch.Value, out Version? latest))
        {
            update.Error = true;
            update.ErrorMessage = "Error parsing version tag";
        }
        else if (latest > _currentVersion)
        {
            update.Available = true;
        }

        return update;
    }
}
