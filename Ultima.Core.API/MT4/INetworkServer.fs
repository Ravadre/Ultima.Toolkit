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

    member __.OnPrice = socket.Packets.Price
    member __.OnCommandResult = socket.Packets.CommandResult
    member __.OnOrders = socket.Packets.UpdateOrders.Select(fun o -> List<_>(o.orders |> Seq.map(fun x -> x.ToOrder())))
    member __.OnHistory = socket.Packets.HistoryOrderInfo
    member __.Disconnected = socket.Disconnected

    member __.Company = company

    member __.RegisterSymbol(symbol: string) = 
        let packet = SymbolRegistrationDTO()
        packet.register <- true
        packet.symbol <- symbol
        socket.Send(packet)

    member __.UnregisterSymbol(symbol: string) = 
        let packet = SymbolRegistrationDTO()
        packet.register <- false
        packet.symbol <- symbol
        socket.Send(packet)

    member __.OpenOrder(order: OpenOrderRequest) = 
        let packet = OpenOrderCommandDTO()
        packet.command <- GetId()
        packet.comment <- order.Comment
        packet.magicNumber <- order.MagicNumber
        packet.openPrice <- order.RequestOpenPrice
        packet.retries <- 1
        packet.retrySpanMs <- 0
        packet.slippage <- 5
        packet.stopLoss <- order.StopLoss
        packet.symbol <- order.Symbol
        packet.takeProfit <- order.TakeProfit
        packet.tradeCommand <- int(order.Type)
        packet.volume <- order.Volume

        socket.Send(packet)
    
    member __.CloseOrderBy(order: int, orderBy: int) = 
        let packet = CloseOrderByCommandDTO()
        packet.command <- GetId()
        packet.order <- order
        packet.orderBy <- orderBy
        packet.retries <- 1
        packet.retrySpanMs <- 0
            
        socket.Send(packet)

    member __.CloseOrder(order: int) = 
        let packet = CloseOrderCommandDTO()
        packet.command <- GetId()
        packet.order <- order
        packet.retries <- 1
        packet.retrySpanMs <- 0

        socket.Send(packet)

    member __.ModifyOrder(request: ModifyOrderRequest) = 
        let packet = ModifyOrderCommandDTO()

        packet.command <- GetId()
        packet.retries <- 1
        packet.retrySpanMs <- 0
        packet.openPrice <- request.OpenPrice
        packet.order <- request.Order
        packet.stopLoss <- request.StopLoss
        packet.takeProfit <- request.TakeProfit

        socket.Send(packet)


type INetworkServer = 

    abstract ClientConnected: IObservable<MT4Client> with get
    abstract Started : IObservable<Unit> with get
    abstract Stopped : IObservable<Unit> with get

    abstract GetConnectedClients: unit -> Async<MT4Client[]>
