namespace Ultima

open NLog.FSharp

type UltimaLogger(name: string) = 
    let log = Logger(NLog.LogManager.GetLogger(name))

    new() = 
        let callerType = System.Diagnostics.StackTrace(1, false)
                               .GetFrames().[0]
                               .GetMethod()
                               .DeclaringType
        UltimaLogger(callerType.Name)


    member __.Trace(message) = 
        log.Trace message
    member __.Debug(message) = 
        log.Debug message
    member __.Info(message) = 
        log.Info message
    member __.Warn(message) = 
        log.Warn message
    member __.Error(message) = 
        log.Error message
    member __.Fatal(message) = 
        log.Fatal message

    member __.TraceMsg(message) = 
        log.Trace "%s" message
    member __.DebugMsg(message) = 
        log.Debug "%s" message
    member __.InfoMsg(message) = 
        log.Info "%s" message
    member __.WarnMsg(message) = 
        log.Warn "%s" message
    member __.ErrorMsg(message) =
        log.Error "%s" message
    member __.FatalMsg(message) = 
        log.Fatal "%s" message

    member __.TraceFormat(message, [<System.ParamArrayAttribute>] param) = 
        log.Trace "%s" (System.String.Format(message, param))
    member __.DebugFormat(message, [<System.ParamArrayAttribute>] param) = 
        log.Debug "%s" (System.String.Format(message, param))
    member __.InfoFormat(message, [<System.ParamArrayAttribute>] param) = 
        log.Info "%s" (System.String.Format(message, param))
    member __.WarnFormat(message, [<System.ParamArrayAttribute>] param) = 
        log.Warn "%s" (System.String.Format(message, param))
    member __.ErrorFormat(message, [<System.ParamArrayAttribute>] param) = 
        log.Error "%s" (System.String.Format(message, param))
    member __.FatalFormat(message, [<System.ParamArrayAttribute>] param) = 
        log.Fatal "%s" (System.String.Format(message, param))