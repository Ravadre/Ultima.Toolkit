namespace Ultima

open System
open Ultima.MT4.Packets

type OrderType = 
| Buy = 0
| Sell = 1
| BuyLimit = 2
| SellLimit = 3
| BuyStop = 4
| SellStop = 5

type Order = 
    {
        Id : int64;
        Symbol: string;
        Type: OrderType;
        Volume: float;
        OpenPrice: float;
        StopLoss: float;
        TakeProfit: float;
        CurrentPrice: float;
        Commission: float;
        Swap: float;
        Profit: float;
        PointProfit: int;
        OpenTime: DateTime;
    }
    member this.TypeAsString() = 
        match this.Type with
        | OrderType.Buy -> "buy"
        | OrderType.Sell -> "sell"
        | OrderType.BuyLimit -> "buy limit"
        | OrderType.SellLimit -> "sell limit"
        | OrderType.BuyStop -> "buy stop"
        | OrderType.SellStop -> "sell stop"
        | _ -> "invalid"
    

type OpenOrderRequest = 
    {
        Symbol: string;
        Type: OrderType;
        Volume: float;
        RequestOpenPrice: float;
        StopLoss: float;
        TakeProfit: float;
        Comment: string;
        MagicNumber: int;
    }


[<AutoOpen>]
module OrderDTOExtensions = 
    let epoch = DateTime(1970, 1, 1)
    type OrderInfoDTO with
        member t.ToOrder() = 
            { Id = int64(t.order)
              Symbol = t.symbol
              Type = enum<OrderType>(t.tradeCommand)
              Volume = t.volume
              OpenPrice = t.openPrice
              StopLoss = t.stopLoss
              TakeProfit = t.takeProfit
              CurrentPrice = t.closePrice
              Profit = t.profit
              Commission = t.commission
              Swap = t.swap
              PointProfit = t.pointProfit
              OpenTime = epoch.AddSeconds(float t.openTime) }