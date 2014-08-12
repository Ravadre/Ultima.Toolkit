namespace Ultima

open System
open System.IO
open Newtonsoft.Json
open Newtonsoft.Json.Linq

type Config(configContent: string) = 
    let cfg = (JsonConvert.DeserializeObject(configContent) :?> JObject)
                .GetValue("config") :?> JObject
    
    interface IConfig with
    
        member __.GetSection<'T> (name: string) = 
            cfg.GetValue(name).ToObject<'T>()

type PluginConfig(configContent: string) = 
    let cfg = if not(String.IsNullOrWhiteSpace(configContent)) then
                 (JsonConvert.DeserializeObject(configContent) :?> JObject)
              else
                 null

    interface IPluginConfig with
        member __.HasConfiguration 
            with get() = cfg <> null

        member __.As<'T>() = 
            match cfg with
            | null -> failwith "Plugin has no configuration"
            | _ -> cfg.ToObject<'T>()
    
    static member LoadFromFile(file: string) = 
        try
            match File.Exists(file) with
            | false -> PluginConfig(null) :> IPluginConfig
            | true -> PluginConfig(File.ReadAllText(file)) :> IPluginConfig
        with
        | _ -> PluginConfig(null) :> IPluginConfig