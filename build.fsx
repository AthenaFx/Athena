// include Fake lib
#r @"tools\FAKE\tools\FakeLib.dll"

open Fake
open Fake.DotNet.MsBuild

let projectName = "Athena"
let projectSummary = "Athena framework"
let projectDescription = "Athena framework for building modern services"
let authors = ["Mattias Jakobsson"]

// version info
let version = ReadFileAsString "version.txt" // or retrieve from CI server

Target "AssemblyInfo" (fun _ ->
    AssemblyInfo 
        (fun p -> 
        {p with
            CodeLanguage = CSharp;
            AssemblyVersion = version;
            OutputFileName = @".\src\CommonAssemblyInfo.cs"})             
)

Target "Build" (fun _ ->
    !! @"src\*.sln" 
    |> MSBuildWithDefaults "Build"
    |> Log "Build-Output: "
)

"AssemblyInfo"
  ==> "Build"

// start build
RunTargetOrDefault "Build"