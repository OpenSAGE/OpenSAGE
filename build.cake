#tool "nuget:?package=gitreleasemanager&version=0.6.0"
#tool "nuget:?package=GitVersion.CommandLine&version=3.6.5"

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

var shouldPublish = AppVeyor.IsRunningOnAppVeyor 
    && !AppVeyor.Environment.PullRequest.IsPullRequest
    && AppVeyor.Environment.Repository.Name == "OpenSAGE/OpenSAGE"
    && AppVeyor.Environment.Repository.Branch == "master"
    && AppVeyor.Environment.Repository.Tag.IsTag
    && !string.IsNullOrWhiteSpace(AppVeyor.Environment.Repository.Tag.Name);

Task("Restore")
    .Does(() => {
        DotNetCoreRestore("./src");
    });

Task("Build")
    .IsDependentOn("Restore")
    .Does(() => {
        DotNetCoreBuild("./src", new DotNetCoreBuildSettings {
            Configuration = configuration,
            NoRestore = true,
        });
    });

Task("Test")
    .IsDependentOn("Build")
    .Does(() => {
        var projectFiles = GetFiles("./src/*.Tests/*.csproj");
        foreach (var projectFile in projectFiles) {
            DotNetCoreTest(projectFile.FullPath, new DotNetCoreTestSettings {
                Configuration = configuration,
                NoRestore = true,
                NoBuild = true
            });
        }
    });

Task("CreateBinaries")
    .IsDependentOn("Test")
    .Does(() => {
        var projectFiles = new[] {
            "./src/OpenSage.Launcher/OpenSage.Launcher.csproj",
            "./src/OpenSage.Viewer/OpenSage.Viewer.csproj"
        };
        var runtimes = new[] {
            "win-x64",
            "osx-64"
        };
        foreach (var projectFile in projectFiles) {
            foreach (var runtime in runtimes) {
                DotNetCorePublish(projectFile, new DotNetCorePublishSettings {
                    Configuration = configuration,
                    NoRestore = true,
                    OutputDirectory = "./artifacts",
                    Runtime = runtime
                });
            }
        }
    });

// Task("CreateReleaseNotes")
//     .Does(() => {
//         GitReleaseManagerCreate(parameters.GitHub.UserName, parameters.GitHub.Password, "cake-build", "cake-vscode", new GitReleaseManagerCreateSettings {
//             Milestone         = parameters.Version.Milestone,
//             Name              = parameters.Version.Milestone,
//             Prerelease        = true,
//             TargetCommitish   = "master"
//         });
//     });

Task("Default")
    .IsDependentOn("CreateBinaries");

// Task("AppVeyor")
//     .IsDependentOn("Upload-AppVeyor-Artifacts")
//     .IsDependentOn("Publish-GitHub-Release")
//     .IsDependentOn("Publish-Extension")
//     .Finally(() =>
// {
//     if(publishingError)
//     {
//         throw new Exception("An error occurred during the publishing of cake-vscode.  All publishing tasks have been attempted.");
//     }
// });

RunTarget(target);