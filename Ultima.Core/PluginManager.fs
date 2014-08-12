namespace Ultima

open System
open System.Threading.Tasks
open System.Reflection
open System.Collections.Generic
open Stacks
open Stacks.Actors
open NLog.FSharp

type PluginManager() = 
    let log = Logger()    
    let context = ActorContext("Plugin manager")

    let runningPlugins = List<IPlugin * Task>()

    let LoadPlugin(pluginPath: string) = 
        let asm = Assembly.LoadFrom(pluginPath)
        
        let t = asm.GetTypes()
                |> Seq.tryFind(fun t -> t.IsPublic &&
                                        t.IsClass &&
                                        typeof<IPlugin>.IsAssignableFrom(t))
        match t with
        | Some t -> Some(Activator.CreateInstance(t) :?> IPlugin)
        | None -> log.Info "%s is not an Ultima plugin" pluginPath
                  None


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


    member __.Start(pluginPath: string, services: IUltimaServices) = async {
        do! Async.SwitchToActor(context)
        log.Trace "1. Plugin start on %s" (Executor.GetCurrentName())
        
        try
            match LoadPlugin(pluginPath) with
            | None -> ()
            | Some plugin ->
                plugin.Initialize(services)
                let task = plugin.Run()
                log.Info "Plugin %s (%s) running" (plugin.Info.Name) (plugin.Info.Version.ToString())
                (plugin, task) |> HookTaskEnd
                runningPlugins.Add( (plugin, task) )
        with
        | exn -> log.Error "Cannot start plugin %s because exception was thrown: %O"
                    pluginPath exn
    }

    member __.StopAll() = async {
        do! Async.SwitchToActor(context)
        log.Trace "2. Stop all on %s" (Executor.GetCurrentName())

        runningPlugins
        |> Seq.iter TryStopPlugin
    }