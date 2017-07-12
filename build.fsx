#r @"tools\FAKE\tools\FakeLib.dll"

open Fake

let buildNumber = getBuildParamOrDefault "buildNumber" "1"
let nugetApiKey = getBuildParamOrDefault "nugetKey" ""
let nugetFeedUrl = getBuildParamOrDefault "nugetFeed" ""
let configuration = getBuildParamOrDefault "configuration" "Debug"
let version = ((ReadFileAsString "version.txt") + "." + buildNumber)
let buildDir = "./NuGet"

Target "Clean" (fun _ ->
    !! "src/**/bin"
    ++ "src/**/obj"
    |> CleanDirs

    CleanDirs [buildDir]
)

Target "Restore" (fun _ ->
    DotNetCli.Restore
        (fun p -> 
           { p with 
                WorkingDir = ".\src"})
)

Target "AssemblyInfo" (fun _ ->
    AssemblyInfo
        (fun p -> 
        {p with
            CodeLanguage = CSharp;
            AssemblyVersion = version;
            OutputFileName = @".\src\CommonAssemblyInfo.cs"})             
)

Target "Build" (fun _ ->
    DotNetCli.Build
        (fun p -> 
           { p with 
                Configuration = "Release";
                WorkingDir = ".\src"})

)

Target "CreatePackages" (fun _ -> 
    DotNetCli.Pack
        (fun p -> 
           { p with 
                OutputPath = "../../NuGet";
                Configuration = configuration;
                WorkingDir = ".\src";
                AdditionalArgs = ["/p:PackageVersion=" + version]})
)

Target "PushPackages" (fun _ -> 
    let pushPackage = (fun name ->
        NuGetPublish (fun nugetParams -> 
            { nugetParams with
                AccessKey = nugetApiKey
                PublishUrl = nugetFeedUrl
                Project = name
                Version = version
            }
        )
    )

    pushPackage("Athena")
    pushPackage("Athena.Web")
    pushPackage("Athena.EventStore")
    pushPackage("Athena.Consul")
    pushPackage("Athena.Diagnostics")
)

"Clean"
  ==> "AssemblyInfo"
  ==> "Restore"
  ==> "Build"

"Clean"
  ==> "AssemblyInfo"
  ==> "Restore"
  ==> "CreatePackages"
  ==> "PushPackages"

RunTargetOrDefault "Build"