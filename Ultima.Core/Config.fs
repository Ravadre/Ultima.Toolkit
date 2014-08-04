namespace Ultima

open System.IO
open Newtonsoft.Json
open Newtonsoft.Json.Linq

type Config(configContent: string) = 
    let content = configContent
    let cfg = (JsonConvert.DeserializeObject(configContent) :?> JObject)
                .GetValue("config") :?> JObject

    interface IConfig with
    
        member this.GetSection<'T> (name: string) = 
            cfg.GetValue(name).ToObject<'T>()


