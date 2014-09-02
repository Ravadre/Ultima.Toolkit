namespace Ultima

open System
open System.Collections.Generic
open Ultima.MT4.Packets
open System.Reactive.Subjects
open System.Reactive.Linq

[<Flags>]
type OrderModifications =
| Profit = 0x1
| Type = 0x100
| StopLoss = 0x200
| TakeProfit = 0x400
| Volume = 0x800
| OpenPrice = 0x1000

[<AutoOpen>]
module OrderExtensions = 
    type OrderModifications with
        member this.IsImportant() = 
            int(this) &&& 0xffffff00 <> 0
        member this.HasTypeChanged() = 
            (int(this) &&& int(OrderModifications.Type)) <> 0
        member this.HasVolumeChanged() = 
            (int(this) &&& int(OrderModifications.Volume)) <> 0
        

type OrderDeltaCalculator() = 
    
    let mutable currentOrders = List<OrderInfoDTO>()

    let orderOpened = new Subject<OrderInfoDTO>()
    let orderClosed = new Subject<OrderInfoDTO>()
    let orderModified = new Subject<OrderInfoDTO * OrderInfoDTO * OrderModifications>()
    
    let GetOrderModifications (co: OrderInfoDTO) (no: OrderInfoDTO) = 
        let mutable mods = 0

        if co.openPrice <> no.openPrice then
            mods <- mods ||| int(OrderModifications.OpenPrice)
        if co.stopLoss <> no.stopLoss then
            mods <- mods ||| int(OrderModifications.StopLoss)
        if co.takeProfit <> no.takeProfit then
            mods <- mods ||| int(OrderModifications.TakeProfit)
        if co.tradeCommand <> no.tradeCommand then
            mods <- mods ||| int(OrderModifications.Type)
        if co.volume <> no.volume then
            mods <- mods ||| int(OrderModifications.Volume)
        if co.profit <> no.profit then
            mods <- mods ||| int(OrderModifications.Profit)
        mods


    member __.OrderOpened = orderOpened.AsObservable<_>()
    member __.OrderClosed = orderClosed.AsObservable<_>()
    member __.OrderModified = orderModified.AsObservable<_>()

    member __.Reset() = currentOrders.Clear()


    member __.UpdateOrders (orders: List<OrderInfoDTO>) = 
        orders.Sort(Comparison(fun (l: OrderInfoDTO) r -> l.order - r.order))

        let mutable coe = currentOrders.GetEnumerator()
        let mutable noe = orders.GetEnumerator()

        let mutable cGot = coe.MoveNext()
        let mutable nGot = noe.MoveNext()

        while cGot || nGot do
            if not(nGot) then
                orderClosed.OnNext(coe.Current)
                cGot <- coe.MoveNext()
            elif not(cGot) then
                orderOpened.OnNext(noe.Current)
                nGot <- noe.MoveNext()
            else
                let co = coe.Current
                let no = noe.Current

                if co.order < no.order then
                    orderClosed.OnNext(co)
                    cGot <- coe.MoveNext()
                elif co.order > no.order then
                    orderOpened.OnNext(no)
                    nGot <- noe.MoveNext()
                else
                    let mods = GetOrderModifications co no
                    if mods <> 0 then
                        orderModified.OnNext(co, no, enum<OrderModifications>(mods))
                    cGot <- coe.MoveNext()
                    nGot <- noe.MoveNext()
        
        currentOrders <- orders
