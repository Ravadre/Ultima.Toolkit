namespace Ultima

open System
open System.Threading
open System.Threading.Tasks
open System.Reflection
open System.Collections.Generic
open Stacks
open Stacks.Actors
open Stacks.FSharp
open NLog.FSharp

type PluginManager() = 
    let log = Logger()    
    let context = ActorContext("Plugin manager")

    let runningPlugins = List<IPlugin * Task>()

    let LoadPlugin(pluginPath: string) = 
        let asm = Assembly.LoadFile(pluginPath)
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
        
        let rec TryLoadPlugin path exts = 
            match exts with
            | [] -> PluginConfig(null) :> IPluginConfig
            | ext :: r -> let f = System.IO.Path.ChangeExtension(path, ext)
                          let cfg = PluginConfig.LoadFromFile(f)
                          match cfg.HasConfiguration with
                          | true -> cfg
                          | false -> TryLoadPlugin path r
        let s = ref false
        try
            match LoadPlugin(pluginPath) with
            | None -> ()
            | Some plugin ->
                let pluginCfg = TryLoadPlugin pluginPath [".json"; ".cfg"; ".config"]
                plugin.Initialize(services, pluginCfg)
                let task = plugin.Run()
                log.Info "Plugin %s (%s) running" (plugin.Info.Name) (plugin.Info.Version.ToString())
                (plugin, task) |> HookTaskEnd
                runningPlugins.Add( (plugin, task) )
                s := true
        with
        | exn -> log.Error "Cannot start plugin %s because exception was thrown: %O"
                    pluginPath exn
        return !s
    }

    member __.SignalStop(name: string) = context.RunAsync(async {
        match runningPlugins |> Seq.tryFind(fun (p,_) -> p.Info.Name = name) with
        | Some x -> TryStopPlugin(x)
        | None -> failwithf "No plugin %s running" name
    })

    member __.Stop(name: string, cancel: CancellationToken) = 
        context.RunAsync(async {
            match runningPlugins |> Seq.tryFind(fun (p,_) -> p.Info.Name = name) with
            | Some x -> TryStopPlugin(x)
            | None -> failwithf "No plugin %s running" name

            while (runningPlugins 
                   |> Seq.tryFind(fun (p,_) -> p.Info.Name = name)).IsSome do
                do! Async.Sleep(50)
                cancel.ThrowIfCancellationRequested()
        })

    member __.SignalStopAll() = async {
        do! Async.SwitchToActor(context)
        log.Trace "2. Stop all on %s" (Executor.GetCurrentName())

        runningPlugins
        |> Seq.iter TryStopPlugin
    }

    member __.GetRunningPlugins() = 
        context.RunAsync(async {
            return List.ofSeq (runningPlugins)
        })