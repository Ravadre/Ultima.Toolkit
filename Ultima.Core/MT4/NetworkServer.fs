namespace Ultima.MT4


open System
open System.Net
open System.Collections.Generic
open System.Reactive.Linq
open System.Reactive.Subjects
open Ultima.MT4.Packets
open Stacks
open Stacks.Tcp
open NLog.FSharp
open Ultima

type MT4Socket = ReactiveMessageClient<IMT4PacketHandler>

type NetServerConfig() =
    [<DefaultValue>] 
    val mutable bind: string
    [<DefaultValue>]
    val mutable port: int

type NetworkServer() as ns = 
    let log = Logger()

    let exec = ActionBlockExecutor("MT4 network server")

    [<DefaultValue>]
    val mutable server: SocketServer
    //let server = SocketServer(exec, IPEndPoint(IPAddress.Any, opts.Port))

    let connectedSub = ref null

    let temporarySockets = List<MT4Socket>()
    let clients = Dictionary<string, MT4Client>()

    let clientConnected = new Subject<MT4Client>()

    member this.Initialize(services: IUltimaServices) = 
        let cfg = services.Config.GetSection<NetServerConfig>("mt4Connector")
        this.server <- SocketServer(exec, IPEndPoint(IPAddress.Parse(cfg.bind), cfg.port))
        ()

    member this.Start() = 
        connectedSub := this.server.Connected.Subscribe(this.OnConnected)
        this.server.Start()
        log.Info "Network server started (%s)" (this.server.BindEndPoint.ToString())

    member this.Stop() = 
        log.Info "Network server stopping"
        clientConnected.Dispose()
        (!connectedSub).Dispose()
        this.server.Stop()

    interface INetworkServer with
        member this.ClientConnected with get() = clientConnected.AsObservable()
        member this.Started with get() = this.server.Started
        member this.Stopped with get() = this.server.Stopped

        member this.GetConnectedClients() =
            exec.PostTask(fun () -> clients
                                    |> Seq.map (fun kv -> kv.Value)
                                    |> Array.ofSeq)
            |> Async.AwaitTask


    interface IUltimaService with
        member this.Initialize(services: IUltimaServices) = 
            ns.Initialize(services)

        member this.Start() = 
            ns.Start()

        member this.Stop() = 
            ns.Stop()


    member private this.OnConnected(c) = 
        let c = MT4Socket(FramedClient(c), ProtoBufStacksSerializer())

        c.Packets.Login.Subscribe(fun l ->
            log.Info "Client %s registering as %s" (c.RemoteEndPoint.ToString()) (l.company)
            this.RegisterClient(l.company, c)
        ) |> ignore
        c.Disconnected.Subscribe(fun (e: exn) ->
            temporarySockets.Remove(c) |> ignore
        ) |> ignore

        temporarySockets.Add(c)
        ()

    member private this.RegisterClient(company: string, c) = 
        match clients.ContainsKey(company) with
        | true ->   
            log.Warn "Already has connection with company %s registered. Dropping client %s"
                company (c.RemoteEndPoint.ToString())
            c.Close()
        | false -> 
            if not(temporarySockets.Remove(c)) then
                log.Warn "Connection %s was not in temporary socket list" (c.RemoteEndPoint.ToString())
            
            let mt4Client = MT4Client(c, company)
            clients.Add(company, mt4Client)
            c.Disconnected.Subscribe(fun e -> clients.Remove(company) |> ignore) |> ignore

            clientConnected.OnNext(mt4Client)
        ()