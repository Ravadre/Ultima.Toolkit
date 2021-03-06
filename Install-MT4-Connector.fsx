#r @"Tools\FAKE\tools\FakeLib.dll"

open System
open System.IO
open Fake
open System.Diagnostics

let pluginDir = if (getBuildParamOrDefault "Mode" "Debug")
                    .Equals("Release", StringComparison.OrdinalIgnoreCase) 
                then "Release" else "Debug"
let mt4DataFolder = getBuildParamOrDefault "MT4DataFolder" 
                        (Environment.GetFolderPath(
                            Environment.SpecialFolder.ApplicationData) @@ "MetaQuotes\\Terminal")
let originDiskMap = getBuildParamOrDefault "OriginDiskMap" ""
let mt4Path = getBuildParam "MT4Path"
let portable = getBuildParamOrDefault "Portable" "false"
let clean = getBuildParamOrDefault "Clean" "false"

let isPortable = bool.Parse(portable.ToLowerInvariant())
let isClean = bool.Parse(clean.ToLowerInvariant())

do
    if isPortable then printfn "Running in portable mode"

let NormalizeDir d = 
    Path.GetFullPath(d).TrimEnd('\\', '/')

let findDataDir mt4Path = 
    if not(isPortable) && not(isClean) then
        let r = mt4DataFolder

        Directory.EnumerateDirectories(r)
        |> Seq.tryFind (fun sd ->
            let f = sd @@ "origin.txt"
            let p = if File.Exists(f) then File.ReadAllText(f) else ""

            if p <> "" then
                let p = if originDiskMap = "" then 
                            p 
                        else 
                            logfn "Mapping %s" p
                            let t = originDiskMap + ":" + (p.Split(':').[1])
                            logfn "Mapped data dir from %s to %s" p t
                            t
                NormalizeDir(mt4Path) = NormalizeDir(p)
            else 
                false
        )
    else
        Some mt4Path 

  
let IsMetaTraderRunning() = 
    let terminals = Process.GetProcessesByName("terminal")
    terminals.Length > 0

let KillMetaTrader(directory) = 
    let terminals = Process.GetProcessesByName("terminal")

    for proc in terminals do
        try
            let mt4Dir = NormalizeDir(Path.GetDirectoryName(proc.MainModule.FileName))
            let dir = NormalizeDir(directory)

            if dir = mt4Dir then
                logfn "Killing MT4 terminal in %s" mt4Dir
                proc.Kill()
                proc.WaitForExit()
        with
        | exn -> logfn "Could not kill terminal %s" proc.MainModule.FileName


let StartMetaTrader(directory) = 
    let terminalPath = directory @@ "terminal.exe"

    logfn "Starting Meta Trader..."
    let proc = Process.Start(terminalPath, (if isPortable then "/portable" else ""))
    proc.WaitForInputIdle() |> ignore
    logfn "done"


Target "Deploy" (fun () ->
    if mt4Path = "" then 
        failwith "MT4Path parameter is obligatory"

    if not(Directory.Exists(mt4Path)) then
        if isClean then
            Directory.CreateDirectory(mt4Path) |> ignore
        else
            failwithf "Directory %s does not exists" mt4Path

    match findDataDir mt4Path with
    | None -> failwithf "No data directory found for %s" mt4Path
    | Some dd -> 
        logfn "Data dir: %s" dd
        let dd = dd @@ "MQL4"

        let restartMt4 = IsMetaTraderRunning()
        if (restartMt4) then
            KillMetaTrader(mt4Path)

        if isClean then
            Directory.CreateDirectory(dd) |> ignore
            Directory.CreateDirectory(dd @@ "Include\\") |> ignore
            Directory.CreateDirectory(dd @@ "Libraries\\") |> ignore
            Directory.CreateDirectory(dd @@ "Experts\\") |> ignore
            Directory.CreateDirectory(dd @@ "Images\\") |> ignore

        let srcScriptDir = "Connectors\\MT4\\Scripts\\"
        let filesDir = "files\\"

        let filesToCopy = 
            [ srcScriptDir @@ "Common.mqh", dd @@ "Include\\Common.mqh";
              srcScriptDir @@ "UltimaConnector.Common.mqh", dd @@ "Include\\UltimaConnector.Common.mqh";
              srcScriptDir @@ "UltimaConnector.mqh", dd @@ "Include\\UltimaConnector.mqh";
              srcScriptDir @@ "UltimaConnector.Native.mqh", dd @@ "Include\\UltimaConnector.Native.mqh";
              srcScriptDir @@ "UltimaConnector.UI.mqh", dd @@ "Include\\UltimaConnector.UI.mqh";
              srcScriptDir @@ "Common.mq4", dd @@ "Libraries\\Common.mq4";
              srcScriptDir @@ "UltimaConnector.mq4", dd @@ "Experts\\UltimaConnector.mq4";
              filesDir @@ "logo64.ico", dd @@ "Images\\logo64.ico";
            ]

        let filesToDelete = [
            dd @@ "Libraries\\Common.ex4";
            dd @@ "Experts\\UltimaConnector.ex4";
            ]

        for f in filesToDelete do
            if File.Exists(f) then
                try
                    File.Delete(f)
                    logfn "Deleted %s" f
                with
                | e -> logfn "Could not delete %s" f
        
        for src, dst in filesToCopy do
            if not (File.Exists(src)) then
                failwithf "File %s does not exists" src
            
            let srcStamp = FileInfo(src).LastWriteTimeUtc
            let dstStamp = if File.Exists(dst) then Some(FileInfo(dst).LastWriteTimeUtc) else None

            if dstStamp.IsSome &&
               dstStamp.Value > srcStamp then
               failwithf "File %s was written to later than source file. Aborting" dst

        for src, dst in filesToCopy do
            logfn "Copying %s" (Path.GetFileName(src))
            File.Copy(src, dst, true)


        logfn "Copying UltimaConnector.dll"
        let pluginPath = pluginDir @@ "UltimaConnector.dll"
        File.Copy(pluginPath, dd @@ "Libraries\\UltimaConnector.dll", true)

        if restartMt4 then
            StartMetaTrader(mt4Path)

)

RunTargetOrDefault "Deploy"