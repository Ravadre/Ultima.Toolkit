namespace Ultima

open System
open System.IO
open System.Reactive
open System.Reactive.Linq
open Ultima.MT4.Packets
open NLog.FSharp

type UltimaServices(mt4NetServer: MT4.INetworkServer, 
                    config: IConfig) = 
    interface IUltimaServices with
        member this.MT4NetworkServer = mt4NetServer
        member this.Config = config

type Ultima() = 
    let log = Logger()

    let pluginManager = PluginManager()
    let mt4NetServer = MT4.NetworkServer()
    let config = Config(File.ReadAllText("ultima.json"))
    let services = UltimaServices(mt4NetServer, config) :> IUltimaServices

    member this.Initialize() = 
        (mt4NetServer :> IUltimaService).Initialize(services)
        ()

    member this.Start() = 
        (mt4NetServer :> IUltimaService).Start()
        ()

    member this.Stop() = 
        (mt4NetServer :> IUltimaService).Stop()
        (mt4NetServer :> MT4.INetworkServer).Stopped.Wait() |> ignore
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
