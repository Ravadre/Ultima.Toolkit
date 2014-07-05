namespace TickLogger

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
        member x.Info: PluginInfo = info
        
        member x.Initialize(s: IUltimaServices): unit = 
            log.Info("Initialize called")
            services <- s
        
        member x.Run(): Task = 
            log.Info("Run called")
            runTask.Task :> Task
        
        member x.StatusChanged: System.IObservable<string> = 
            statusChanged.AsObservable()
        
        member x.Stop(): unit = 
            log.Info("Stop called")
            runTask.SetResult()
        
