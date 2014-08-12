namespace Ultima

open Newtonsoft.Json
open Newtonsoft.Json.Linq

type Config(configContent: string) = 
    let cfg = (JsonConvert.DeserializeObject(configContent) :?> JObject)
                .GetValue("config") :?> JObject

    interface IConfig with
    
        member __.GetSection<'T> (name: string) = 
            cfg.GetValue(name).ToObject<'T>()


