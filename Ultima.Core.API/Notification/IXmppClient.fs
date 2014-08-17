namespace Ultima

type IXmppClient =
    abstract Send: message: string -> unit

