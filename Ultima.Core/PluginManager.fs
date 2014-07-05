namespace Ultima

open System
open System.Threading.Tasks
open System.Reflection
open System.Collections.Generic
open Stacks
open Stacks.Actors
open NLog.FSharp

type PluginInfo = {
    Name: string
    Version: Version
}

type IPlugin = 
    abstract Initialize: services: IUltimaServices -> unit
    abstract Run: unit -> unit
    abstract Stop: unit -> unit
    
    abstract Info: PluginInfo with get

    abstract StatusChanged: IObservable<string> with get



type PluginManager() = 
    let log = Logger()    
    let context = ActorContext("Plugin manager")

    let runningPlugins = List<IPlugin * Task>()

    let LoadPlugin(pluginPath: string) = 
        let asm = Assembly.LoadFrom(pluginPath)
        
        let t = asm.GetTypes()
                |> Seq.find(fun t -> t.IsPublic &&
                                     t.IsClass &&
                                     typeof<IPlugin>.IsAssignableFrom(t))
        Activator.CreateInstance(t) :?> IPlugin

    let HookTaskEnd (plugin: IPlugin, task: Task) = 
        task.ContinueWith( (fun (t: Task) -> 
            let wf = async {
                        do! Async.SwitchToActor(context)
                        log.Trace "3. hook task end callback on %s" (Executor.GetCurrentName())

                        match t.Exception with
                        | null -> log.Info "Plugin %s finished gracefully" (plugin.Info.Name)
                        | exn -> log.Error "Plugin %s finished with exception: %O" (plugin.Info.Name) exn

                        runningPlugins.Remove( (plugin, task) ) |> ignore
                     }
            Async.StartImmediate wf) 
        ) |> ignore


    let TryStopPlugin (plugin: IPlugin, task: Task) = 
        try
            plugin.Stop()
        with
        | exn -> log.Error "Plugin %s threw exception when asked to stop, error: %O" (plugin.Info.Name) exn
                 runningPlugins.Remove( (plugin, task) ) |> ignore


    member this.Start(pluginPath: string, services: IUltimaServices) = async {
        do! Async.SwitchToActor(context)
        log.Trace "1. Plugin start on %s" (Executor.GetCurrentName())
        
        try
            let plugin = LoadPlugin(pluginPath)
            let task = Task.Factory.StartNew( 
                        (fun () -> 
                            plugin.Initialize(services)
                            plugin.Run()), 
                        TaskCreationOptions.LongRunning)
            (plugin, task) |> HookTaskEnd
            runningPlugins.Add( (plugin, task) )
        with
        | exn -> log.Error "Cannot start plugin %s because exception was thrown: %O"
                    pluginPath exn
    }

    member this.StopAll() = async {
        do! Async.SwitchToActor(context)
        log.Trace "2. Stop all on %s" (Executor.GetCurrentName())

        runningPlugins
        |> Seq.iter TryStopPlugin
    }