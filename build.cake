#tool "nuget:?package=OctopusTools"

var OctopusURL = "http://localhost:8065";
var OctopusAPIKey = "API-0VISTG2BHEFR0AWMXPIAW4D15U";

var target = Argument("target", "Default");

var packageversion = Argument("packageversion","defaultversion");

var publishDir = MakeAbsolute(Directory("./publishDir")).ToString();
var solutionFile = MakeAbsolute(File("./source/PrintTime.sln"));

Task("Clean")  
  .Does(() =>
{
    Information("Cleaning up {0}",publishDir);
    CleanDirectories(publishDir);
});

Task("Restore")
  .IsDependentOn("Clean")
  .Does(() =>
{    
    Information("Restoring packages for {0}", solutionFile);
    NuGetRestore(solutionFile);
});

Task("PublishToLocalDir")
  .IsDependentOn("Restore")
  .Does(() =>
    {
      Information("Building [{0}] and publishing build output to [{1}]",solutionFile,publishDir);
      MSBuild(solutionFile, new MSBuildSettings()
        .WithProperty("OutDir", publishDir)
      );
      
      Information("Copying more files that will be needed on the package");
      CopyFiles("./templates/*", publishDir);      
    });

Task("CreatePackage")
  .IsDependentOn("PublishToLocalDir")
  .Does(() => {
      OctoPack("PrintTime", new OctopusPackSettings{
      Version = packageversion,
      BasePath = publishDir,
      OutFolder = publishDir
      });
  });

Task("PushPackage")
.IsDependentOn("CreatePackage")
.Does(() => {
  var PackagePath = MakeAbsolute(File(publishDir + "\\PrintTime." + packageversion + ".nupkg"));
  
  OctoPush(OctopusURL,OctopusAPIKey,PackagePath, new OctopusPushSettings{
    ReplaceExisting = true
  });
});

Task("Default")
  .IsDependentOn("PushPackage")
  .Does(() =>
{
  Information("Done!");
});

RunTarget(target);