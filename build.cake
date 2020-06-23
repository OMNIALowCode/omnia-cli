#tool "nuget:?package=GitVersion.CommandLine&version=5.0.1"

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

var solutionFile = "./Omnia.CLI.sln";
var publishFolder = "./publish";

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Task("Clean")
.Does(()=>
{
    CleanDirectory(publishFolder);
});

Task("Version")
    .IsDependentOn("Clean")
    .WithCriteria(() => BuildSystem.IsRunningOnAzurePipelinesHosted)
    .Does(() => {
		var propsFile = "./src/Directory.build.props";
        var readedVersion = XmlPeek(propsFile, "//Version");

        GitVersion(new GitVersionSettings {
		    UpdateAssemblyInfo = true,
			OutputType = GitVersionOutput.BuildServer,
        });

        var gitVersionSettings = new GitVersionSettings {
            OutputType = GitVersionOutput.Json
        };

        var gitVersion = GitVersion(gitVersionSettings);

        var version = gitVersion.SemVer;
        
        Information($"Updating Azure DevOps Pipeline version to version {version}");
                
		XmlPoke(propsFile, "//Version", version);
});


Task("Build")
.IsDependentOn("Version")
.Does(()=>
{
    DotNetCoreBuild(solutionFile, new DotNetCoreBuildSettings()
    {
        Configuration = configuration
    });
});

Task("Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    var projectFiles = GetFiles("./test/**/*.csproj");
	var settings = new DotNetCoreTestSettings
	{
		Filter = "Category!=Ignore",
		Configuration = configuration,
		ArgumentCustomization = args=>args.Append($"--no-build")
	};
	
    foreach(var file in projectFiles)
    {
		DotNetCoreTest(file.FullPath, settings);
    }
});
	

Task("Publish")
.IsDependentOn("Tests")
.Does(()=>
{
    DotNetCorePack(solutionFile, new DotNetCorePackSettings()
    {
        Configuration = configuration,
        OutputDirectory = publishFolder
    });
});

Task("Default")
.IsDependentOn("Publish");

RunTarget(target);