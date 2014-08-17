namespace Ultima

open System.Threading.Tasks


type IUltimaServices =
    abstract MT4NetworkServer: MT4.INetworkServer with get
    abstract Config: IConfig with get
    abstract Xmpp: IXmppClient with get

type IUltimaService = 
    abstract Initialize: IUltimaServices -> unit
    abstract Start: unit -> unit
    abstract Stop: unit -> Task<unit>    