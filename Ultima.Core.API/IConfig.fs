namespace Ultima

type IConfig = 
     abstract GetSection<'T> : string -> 'T

type IPluginConfig = 
     abstract HasConfiguration : bool with get
     abstract As<'T> : unit -> 'T 
     