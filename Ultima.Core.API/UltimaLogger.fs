namespace Ultima

open System
open NLog.FSharp

type UltimaLogger(name: string) = 
    let log = Logger(NLog.LogManager.GetLogger(name))

    new() = 
        let callerType = System.Diagnostics.StackTrace(1, false)
                               .GetFrames().[0]
                               .GetMethod()
                               .DeclaringType
        UltimaLogger(callerType.Name)


    member this.Trace(message) = 
        log.Trace message
    member this.Debug(message) = 
        log.Debug message
    member this.Info(message) = 
        log.Info message
    member this.Warn(message) = 
        log.Warn message
    member this.Error(message) = 
        log.Error message
    member this.Fatal(message) = 
        log.Fatal message

    member this.TraceMsg(message) = 
        log.Trace "%s" message
    member this.DebugMsg(message) = 
        log.Debug "%s" message
    member this.InfoMsg(message) = 
        log.Info "%s" message
    member this.WarnMsg(message) = 
        log.Warn "%s" message
    member this.ErrorMsg(message) =
        log.Error "%s" message
    member this.FatalMsg(message) = 
        log.Fatal "%s" message

    member this.TraceFormat(message, [<System.ParamArrayAttribute>] param) = 
        log.Trace "%s" (System.String.Format(message, param))
    member this.DebugFormat(message, [<System.ParamArrayAttribute>] param) = 
        log.Debug "%s" (System.String.Format(message, param))
    member this.InfoFormat(message, [<System.ParamArrayAttribute>] param) = 
        log.Info "%s" (System.String.Format(message, param))
    member this.WarnFormat(message, [<System.ParamArrayAttribute>] param) = 
        log.Warn "%s" (System.String.Format(message, param))
    member this.ErrorFormat(message, [<System.ParamArrayAttribute>] param) = 
        log.Error "%s" (System.String.Format(message, param))
    member this.FatalFormat(message, [<System.ParamArrayAttribute>] param) = 
        log.Fatal "%s" (System.String.Format(message, param))