namespace Ultima

open Ultima.MT4.Packets

module Order = 
    
    let OperationAsString (order: OrderInfoDTO) = 
        match order.tradeCommand with
        | 0 -> "buy"
        | 1 -> "sell"
        | 2 -> "buy limit"
        | 3 -> "sell limit"
        | 4 -> "buy stop"
        | 5 -> "sell stop"
        | _ -> "invalid"