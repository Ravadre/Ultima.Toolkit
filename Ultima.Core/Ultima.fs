namespace Ultima

open System.IO
open System.Reactive.Linq
open NLog.FSharp

type UltimaServices(mt4NetServer: MT4.INetworkServer, 
                    config: IConfig,
                    xmpp: IXmppClient) = 
    interface IUltimaServices with
        member _t.MT4NetworkServer = mt4NetServer
        member _t.Config = config
        member __.Xmpp = xmpp

type Ultima() = 
    let log = Logger()

    let pluginManager = PluginManager()
    let mt4NetServer = MT4.NetworkServer()
    let config = Config(File.ReadAllText("ultima.json"))
    let xmpp = XmppClient()
    let services = UltimaServices(mt4NetServer, config, xmpp) :> IUltimaServices

    member __.Initialize() = 
        (mt4NetServer :> IUltimaService).Initialize(services)
        (xmpp :> IUltimaService).Initialize(services)
        ()

    member __.Start() = 
        (mt4NetServer :> IUltimaService).Start()
        (xmpp :> IUltimaService).Start()
        ()

    member __.Stop() = 
        (mt4NetServer :> IUltimaService).Stop().Wait()
        (mt4NetServer :> MT4.INetworkServer).Stopped.Wait() |> ignore
        (xmpp :> IUltimaService).Stop().Wait()
        ()

    member __.LoadPluginsFromDirectory(dir: string) = 
        if not(Directory.Exists dir) then
            log.Warn "Directory %s does not exists. Plugins won't be loaded" dir
        else
            Directory.EnumerateFiles(dir, "*.dll")
            |> Seq.iter(fun file ->
                try
                    if Async.RunSynchronously <| pluginManager.Start(file, services) then 
                        log.Info "Plugin started"
                with
                | _ -> log.Warn "Could not start plugin %s" file)        
