#r @"Tools\FAKE\tools\FakeLib.dll"

open Fake
open System.IO

let Configuration = getBuildParamOrDefault "Release"


Target "Clean" (fun() ->
    !! "Ultima.Toolkit.sln"
    |> MSBuild "" "Clean" [ ("Configuration", "Release"); ("Platform", "Mixed Platforms")]
    |> ignore
)


Target "Build" (fun () ->
    !! "Ultima.Toolkit.sln"
    |> MSBuild "" "Build" [ ("Configuration", "Release"); ("Platform", "Mixed Platforms")]
    |> ignore
)

Target "Rebuild" DoNothing

Target "InstallSamplePlugins" (fun () ->
    !! "Ultima.Toolkit.sln"
    |> MSBuild "" "Build" [ ("Configuration", "Debug"); ("Platform", "Mixed Platforms")]
    |> ignore


    let pluginDir = @"Ultima\bin\Debug\plugins"
    let plugins = [ @"SamplePlugins\TickLogger\bin\Debug\TickLogger.dll" ]
    FileHelper.CreateDir pluginDir

    for plugin in plugins do
        FileHelper.CopyFile (pluginDir @@ (Path.GetFileName(plugin))) (plugin)
)

"Rebuild" <== [ "Clean"; "Build" ]

RunTargetOrDefault "Build"