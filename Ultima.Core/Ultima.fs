namespace Ultima

open System.IO
open System.Reactive.Linq
open NLog.FSharp

type UltimaServices(mt4NetServer: MT4.INetworkServer, 
                    config: IConfig) = 
    interface IUltimaServices with
        member _t.MT4NetworkServer = mt4NetServer
        member _t.Config = config

type Ultima() = 
    let log = Logger()

    let pluginManager = PluginManager()
    let mt4NetServer = MT4.NetworkServer()
    let config = Config(File.ReadAllText("ultima.json"))
    let services = UltimaServices(mt4NetServer, config) :> IUltimaServices

    member __.Initialize() = 
        (mt4NetServer :> IUltimaService).Initialize(services)
        ()

    member __.Start() = 
        (mt4NetServer :> IUltimaService).Start()
        ()

    member __.Stop() = 
        (mt4NetServer :> IUltimaService).Stop()
        (mt4NetServer :> MT4.INetworkServer).Stopped.Wait() |> ignore
        ()

    member __.LoadPluginsFromDirectory(dir: string) = 
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
                | _ -> log.Warn "Could not start plugin %s" file)        
