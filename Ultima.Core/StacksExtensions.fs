[<AutoOpen>]
module ActorContextExtensions

type Async with
    static member SwitchToActor(context: Stacks.Actors.ActorContext) = 
        Async.SwitchToContext(context.Context)
    static member SwitchToExecutor(exec: Stacks.IExecutor) = 
        Async.SwitchToContext(exec.Context)
