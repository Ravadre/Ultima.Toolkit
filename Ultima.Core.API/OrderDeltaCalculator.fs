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
    
    let mutable currentOrders = List<Order>()

    let orderOpened = new Subject<Order>()
    let orderClosed = new Subject<Order>()
    let orderModified = new Subject<Order * Order * OrderModifications>()
    
    let GetOrderModifications (co: Order) (no: Order) = 
        let mutable mods = 0

        if co.OpenPrice <> no.OpenPrice then
            mods <- mods ||| int(OrderModifications.OpenPrice)
        if co.StopLoss <> no.StopLoss then
            mods <- mods ||| int(OrderModifications.StopLoss)
        if co.TakeProfit <> no.TakeProfit then
            mods <- mods ||| int(OrderModifications.TakeProfit)
        if co.Type <> no.Type then
            mods <- mods ||| int(OrderModifications.Type)
        if co.Volume <> no.Volume then
            mods <- mods ||| int(OrderModifications.Volume)
        if co.Profit <> no.Profit then
            mods <- mods ||| int(OrderModifications.Profit)
        mods


    member __.OrderOpened = orderOpened.AsObservable<_>()
    member __.OrderClosed = orderClosed.AsObservable<_>()
    member __.OrderModified = orderModified.AsObservable<_>()

    member __.Reset() = currentOrders.Clear()


    member __.UpdateOrders (orders: List<Order>) = 
        orders.Sort(Comparison(fun (l: Order) r -> int(l.Id - r.Id)))

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

                if co.Id < no.Id then
                    orderClosed.OnNext(co)
                    cGot <- coe.MoveNext()
                elif co.Id > no.Id then
                    orderOpened.OnNext(no)
                    nGot <- noe.MoveNext()
                else
                    let mods = GetOrderModifications co no
                    if mods <> 0 then
                        orderModified.OnNext(co, no, enum<OrderModifications>(mods))
                    cGot <- coe.MoveNext()
                    nGot <- noe.MoveNext()
        
        currentOrders <- orders
