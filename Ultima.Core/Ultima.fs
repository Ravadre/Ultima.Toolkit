namespace Ultima

open System
open System.IO
open Ultima.MT4.Packets
open NLog.FSharp

type UltimaServices() = 
    interface IUltimaServices

type Ultima() = 
    let log = Logger()
    let pluginManager = PluginManager()
    let services = UltimaServices() :> IUltimaServices

    member this.Start() = 
        ()

    member this.Stop() = 
        ()

    member this.LoadPluginsFromDirectory(dir: string) = 
        if not(Directory.Exists dir) then
            log.Warn "Directory %s does not exists. Plugins won't be loaded" dir
        else
            Directory.EnumerateFiles(dir)
            |> Seq.iter(fun file ->
                try
                    log.Info "Starting plugin %s" file
                    Async.RunSynchronously <| pluginManager.Start(file, services)    
                    log.Info "Plugin started"
                with
                | exn -> log.Warn "Could not start plugin %s" file)        
