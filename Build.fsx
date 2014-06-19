#r @"Tools\FAKE\tools\FakeLib.dll"

open Fake
open System


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

RunTargetOrDefault "Build"