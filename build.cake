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
.Does(() => {
    var propsFile = "./src/Directory.build.props";
    var readedVersion = XmlPeek(propsFile, "//Version");
	var currentVersion = new Version(readedVersion);
	Information($"Obtained {currentVersion.ToString()} from file.");
	var semVersion = new Version(currentVersion.Major, currentVersion.Minor, currentVersion.Build + 1);
	var version = semVersion.ToString();
	Information($"Generating manual build with version {version}");
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

Task("Publish")
.IsDependentOn("Build")
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