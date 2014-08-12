namespace Ultima

open System
open System.Threading.Tasks

type PluginInfo = {
    Name: string
    Version: Version
}

type IPlugin = 
    abstract Initialize: services: IUltimaServices * config: IPluginConfig -> unit
    abstract Run: unit -> Task
    abstract Stop: unit -> unit
    
    abstract Info: PluginInfo with get

    abstract StatusChanged: IObservable<string> with get




