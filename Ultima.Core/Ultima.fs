namespace Ultima

open System
open System.IO
open System.Reactive
open System.Reactive.Linq
open Ultima.MT4.Packets
open NLog.FSharp

type UltimaServices(mt4NetServer: MT4.INetworkServer) = 
    interface IUltimaServices with
        member this.MT4NetworkServer = mt4NetServer

type Ultima() = 
    let log = Logger()

    let pluginManager = PluginManager()
    let mt4NetServer = MT4.NetworkServer( { Port = 6300 }) :> MT4.INetworkServer
    let services = UltimaServices(mt4NetServer) :> IUltimaServices

    member this.Start() = 
        mt4NetServer.Start()
        ()

    member this.Stop() = 
        mt4NetServer.Stop()
        mt4NetServer.Stopped.Wait() |> ignore
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
