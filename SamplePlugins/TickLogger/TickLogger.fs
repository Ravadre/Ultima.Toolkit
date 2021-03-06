﻿namespace TickLogger

open System
open System.Reactive.Linq
open System.Reactive.Subjects
open System.Threading.Tasks
open NLog.FSharp
open Ultima

type TickLogger() = 
    let log = Logger()
    let info = { Name = "Tick logger"; Version = Version("0.1.0") }

    let mutable services: IUltimaServices = Unchecked.defaultof<_>
    let runTask = TaskCompletionSource<unit>()
    let statusChanged = new Subject<string>()

    interface IPlugin with
        member __.Info: PluginInfo = info
        
        member __.Initialize(s: IUltimaServices, _: IPluginConfig): unit = 
            log.Info("Initialize called")
            services <- s
        
        member __.Run(): Task = 
            log.Info("Run called")
            services.MT4NetworkServer.ClientConnected.Subscribe(fun c ->
                c.OnPrice.Subscribe(fun p ->
                    printfn "Received tick %s - %f / %f" (p.symbol) (p.bid) (p.ask))
                |> ignore

                c.RegisterSymbol("EURUSD")
                c.RegisterSymbol("XAUUSD")
                c.RegisterSymbol("EURPLN")
                
            ) |> ignore


            runTask.Task :> Task
        


        member __.StatusChanged: System.IObservable<string> = 
            statusChanged.AsObservable()
        


        member __.Stop(): unit = 
            log.Info("Stop called")
            runTask.SetResult()
        
