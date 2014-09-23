namespace Ultima

open Stacks.FSharp
open Stacks.Actors
open System.Collections.Generic
open System.Reactive.Subjects
open System.Reactive.Linq
open agsXMPP
open agsXMPP.protocol.client

type XmppClient() as this = 
    let log = UltimaLogger()
    let actor = ActorContext()

    let clients = List<string>()

    let messageReceived = new Subject<XmppMessage>()

    [<DefaultValue>]
    val mutable config: XmppConfigSection
    [<DefaultValue>]
    val mutable con: XmppClientConnection

    let AuthError(_) = 
        log.Error "xmpp service auth error"
        ()

    let Login() = 
        log.Info "xmpp service logged in"

        actor.RunAsync(async {
            this.con.Send(Presence(ShowType.chat, this.config.Presense))
        }) |> Async.StartImmediate

    let Disconnect(error: exn) = 
        log.Warn "xmpp service disconnected. Error: %O" error

        actor.RunAsync(async {
            do! Async.Sleep(10000)
            this.con.Open()
        }) |> Async.StartImmediate

    let Message (msg: Message) = 
        if msg.Type = MessageType.chat &&
           not(System.String.IsNullOrWhiteSpace(msg.Body)) then
            log.Info "Received xmpp message [%s@%s]: %s" 
                msg.From.User msg.From.Server msg.Body
            messageReceived.OnNext({ Sender = msg.From.User + "@" + msg.From.Server;
                                     Message = msg.Body })

    interface IXmppClient with
        member __.Send(message: string): unit = 
            actor.RunAsync(async {
                clients
                |> Seq.iter(fun client ->
                    this.con.Send(agsXMPP.protocol.client.Message(client, MessageType.chat, message)))
            }) |> Async.StartImmediate

        member __.MessageReceived = messageReceived.AsObservable()
        

    interface IUltimaService with
        member this.Initialize(services) =    
            this.config <- services.Config.GetSection<XmppConfigSection>("xmpp")
            this.con <- XmppClientConnection()

            clients.AddRange(this.config.Subscribers)

            ()
        
        member this.Start() = 
            if this.config.Enabled = true then
                actor.RunAsync(async {
                    let jid = Jid(this.config.Account)
                    let c = this.con
                    c.Server <- jid.Server
                    c.Username <- jid.User
                    c.AutoResolveConnectServer <- true
                    c.Password <- this.config.Password
                    c.OnAuthError.Add(AuthError)
                    c.OnLogin.Add(Login)
                    c.OnClose.Add(fun () -> Disconnect(null))
                    c.OnError.Add(Disconnect)
                    c.OnMessage.Add(Message)

                    c.Open()
                })
                |> Async.StartImmediate
            ()

        member __.Stop() = 
            actor.RunAsync(async {
                if this.config.Enabled = true then
                    try
                        this.con.Send(Presence(ShowType.NONE, ""))
                    with
                    | _ -> ()
                    this.con.Close()
            })
            |> Async.StartAsTask
