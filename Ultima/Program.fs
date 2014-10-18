namespace Ultima

open System
open System.IO
open System.ServiceProcess

open NLog.FSharp

module Program = 

    type Arguments = {
        RunService: bool
    }

    let ParseArgs argv =
        let mutable args : Arguments = { 
            RunService = false }

        for a in argv do
            match a with
            | "/s" -> args <- { args with 
                                   RunService = true }
                      ()
            | _ -> printfn "Error: unknown argument %s" a; Environment.Exit(1)
        args


    type UltimaService() = 
        inherit ServiceBase()

        let log = Logger()
        let u = new Ultima()

        override __.OnStart(argv: string[]) = 
            log.Info("Starting Ultima service")
                        
            u.Start()
            u.LoadPluginsFromDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins"))

            base.OnStart(argv)


        override __.OnStop() = 
            log.Info("Ultima service is stopping")

            u.Stop()

            base.OnStop()



    [<EntryPoint>]
    let main argv =
        let args = ParseArgs argv

        match args.RunService with
        | true -> ServiceBase.Run(new UltimaService())
        | false -> 
            try
                Console.SetWindowSize(140, 25)
            with
            | _ -> ()

            let u = new Ultima()
            u.Initialize()
            u.Start()
            u.LoadPluginsFromDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins"))
            printfn "Press Ctrl+C to quit"
            let running = ref true;
            
            Console.CancelKeyPress.Add(fun t ->
                t.Cancel <- true
                running := false)
            
            while !running do
                System.Threading.Thread.Sleep(100)

                
            printfn "Quitting..."
            u.Stop()            
            
            ()
                  
        0