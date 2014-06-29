namespace Ultima

open System
open System.ServiceProcess

open NLog.FSharp

module Program = 

    type Arguments = {
        RunService: bool
    }

    let ParseArgs argv =
        let args : Arguments = { 
            RunService = false }

        for a in argv do
            match a with
            | "/s" -> let args = { args with 
                                    RunService = true }
                      ()
            | _ -> printfn "Error: unknown argument %s" a; Environment.Exit(1)
        args


    type UltimaService() = 
        inherit ServiceBase()

        let log = Logger()
        let u = new Ultima()

        override this.OnStart(argv: string[]) = 
            log.Info("Starting Ultima service")
                        
            u.Start()

            base.OnStart(argv)


        override this.OnStop() = 
            log.Info("Ultima service is stopping")

            u.Stop()

            base.OnStop()



    [<EntryPoint>]
    let main argv =
        let args = ParseArgs argv

        match args.RunService with
        | true -> ServiceBase.Run(new UltimaService())
        | false -> 
            let u = new Ultima()
            u.Start()
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