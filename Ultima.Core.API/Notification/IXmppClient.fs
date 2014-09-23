namespace Ultima

type XmppMessage = {
    Sender: string;
    Message: string
}

type IXmppClient =
    abstract Send: message: string -> unit
    abstract MessageReceived: System.IObservable<XmppMessage>

