namespace Ultima

type XmppConfigSection() = 
    member val Enabled: bool = false with get, set
    member val Account: string = "" with get, set
    member val Password: string = "" with get, set
    member val Subscribers: string[] = [||] with get, set
    member val Presense: string = "" with get, set
