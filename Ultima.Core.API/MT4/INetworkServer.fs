namespace Ultima.MT4

open System
open System.Reactive
open System.Collections.Generic
open Stacks
open System.Reactive.Linq
open Stacks.Tcp
open Ultima
open Ultima.MT4.Packets

type NetworkServerOpts = {
    Port: int
}

type MT4Socket = ReactiveMessageClient<IMT4PacketHandler>

type MT4Client(socket: MT4Socket, company: string) = 
    let mutable cmdId = 0
    let socket = socket
    let company = company

    let GetId() = System.Threading.Interlocked.Increment(&cmdId)

    member this.OnPrice = socket.Packets.Price
    member this.OnCommandResult = socket.Packets.CommandResult
    member this.OnOrders = socket.Packets.UpdateOrders.Select(fun o -> List<_>(o.orders |> Seq.map(fun x -> x.ToOrder())))
    member this.OnHistory = socket.Packets.HistoryOrderInfo
    member this.Disconnected = socket.Disconnected

    member this.Company = company

    member this.RegisterSymbol(symbol: string) = 
        let packet = SymbolRegistrationDTO()
        packet.register <- true
        packet.symbol <- symbol
        socket.Send(packet)

    member this.UnregisterSymbol(symbol: string) = 
        let packet = SymbolRegistrationDTO()
        packet.register <- false
        packet.symbol <- symbol
        socket.Send(packet)


type INetworkServer = 

    abstract ClientConnected: IObservable<MT4Client> with get
    abstract Started : IObservable<Unit> with get
    abstract Stopped : IObservable<Unit> with get

    abstract GetConnectedClients: unit -> Async<MT4Client[]>
