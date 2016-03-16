#tool "ilmerge"
#tool "GitVersion.CommandLine"

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var semVersion = GitVersion(new GitVersionSettings {
                    UpdateAssemblyInfoFilePath = "./src/SolutionInfo.cs",
                    UpdateAssemblyInfo = true
                });

var binDir = Directory("./src/Cake.TeamCity/bin") + Directory(configuration);
var buildResultDir = Directory("./build") + Directory("v" + semVersion.FullSemVer);
var buildBinDir = buildResultDir + Directory("bin");
var buildNugetDir = buildResultDir + Directory("nuget");
var projectSln = File("./src/Cake.TeamCity.sln");

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(() =>
{
    Information("Building version {0} of Cake.TeamCity.", semVersion.FullSemVer);
});

Teardown(() =>
{
    Information("Finished building version {0} of Cake.TeamCity.", semVersion.FullSemVer);
});

///////////////////////////////////////////////////////////////////////////////
// Tasks
///////////////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectories(new DirectoryPath[] { buildResultDir, buildBinDir, buildNugetDir });
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore(projectSln);
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    MSBuild(projectSln, settings =>
        settings.SetConfiguration(configuration));
});

Task("Merge")
    .IsDependentOn("Build")
    .Does(() => 
{
    const string addinName = "Cake.TeamCity.dll";
    var assemblyPaths = GetFiles(binDir.ToString() + "/*.dll").Where(file => !file.FullPath.EndsWith(addinName, StringComparison.OrdinalIgnoreCase));
    ILMerge(buildBinDir + File(addinName), binDir + File(addinName), assemblyPaths);
});

Task("Pack")
    .IsDependentOn("Merge")
    .Does(() => 
{
    var nugetFiles = GetFiles(buildBinDir.ToString() + "/*.*").Select(filePath => new NuSpecContent { Source = filePath.FullPath, Target = "bin" });
    NuGetPack(new NuGetPackSettings {
       Id = "Cake.TeamCity",
       Version = semVersion.NuGetVersion,
       Title = "Cake TeamCity Helpers",
       Authors = new [] { "Richard Simpson" },
       Owners = new [] { "Richard Simpson" },
       Description = "A set of cake helpers for interacting with TeamCity beyond the built in functionality.",
       ProjectUrl = new Uri("https://github.com/RichiCoder1/Cake.TeamCity"),
       LicenseUrl = new Uri("https://github.com/RichiCoder1/Cake.TeamCity/blob/master/LICENSE"),
       Copyright = "Richard Simpson 2016",
       Tags = new [] { "Cake", "TeamCity" },
       Files = nugetFiles.ToList(),
       OutputDirectory = buildNugetDir,
       Symbols = true
    });
});

Task("Default")
  .IsDependentOn("Build");

RunTarget(target);