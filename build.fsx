// include Fake lib
#r @"tools\FAKE\tools\FakeLib.dll"

open Fake
open Fake.DotNet.MsBuild

let projectName = "Athena"
let projectSummary = "Athena framework"
let projectDescription = "Athena framework for building modern services"
let authors = ["Mattias Jakobsson"]

// version info
let buildNumber = getBuildParamOrDefault "buildNumber" "0"
let nugetApiKey = getBuildParamOrDefault "nugetKey" ""
let nugetFeedUrl = getBuildParamOrDefault "nugetFeed" ""
let version = (ReadFileAsString "version.txt") + "." + buildNumber
let buildDir = "./build"

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
                OutputPath = "../../build";
                Configuration = "Release";
                WorkingDir = ".\src"})
)

Target "PushPackages" (fun _ -> 
    NuGetPublish (fun nugetParams -> 
        { nugetParams with
            AccessKey = nugetApiKey
            PublishUrl = nugetFeedUrl
            Project = "Athena"
            Version = version
            WorkingDir = ".\build"
        }
    )
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

// start build
RunTargetOrDefault "Build"