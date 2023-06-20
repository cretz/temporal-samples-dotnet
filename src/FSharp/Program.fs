open System
open System.Threading
open Microsoft.Extensions.Logging
open Temporalio.Client
open Temporalio.Worker
open TemporalioSamples.FSharp
open TemporalioSamples.FSharp.FSharpExtensions

let createClient () =
    TemporalClient.ConnectAsync(TemporalClientConnectOptions(
        "localhost:7233",
        LoggerFactory = LoggerFactory.Create(fun builder ->
            builder
                .AddSimpleConsole(fun opts -> opts.TimestampFormat <- "[HH:mm:ss] ")
                .SetMinimumLevel(LogLevel.Information) |> ignore)))

let runWorker () = async {
    // Cancel on ctrl+c
    use tokenSource = new CancellationTokenSource ()
    Console.CancelKeyPress.Add(
        fun eventArgs -> tokenSource.Cancel (); eventArgs.Cancel <- true)

    // Create activities instance
    let activities = MyActivities ()

    // Build client and worker options
    let! client = createClient() |> Async.AwaitTask
    let opts = TemporalWorkerOptions(TaskQueue = "fsharp-sample")
    opts.addActivity(activities.selectFromDatabase)
    opts.addActivity(MyActivities.doStaticThing)
    opts.addWorkflow<MyWorkflow>()

    // Run worker until cancelled
    printfn "Running worker"
    use worker = new TemporalWorker(client, opts)
    try
        return! worker.ExecuteAsync(tokenSource.Token) |> Async.AwaitTask
    with
        | :? OperationCanceledException -> printfn "Worker cancelled"
}

let executeWorkflow () = async {
    printfn "Executing workflow"
    let! client = createClient() |> Async.AwaitTask
    let! result = client.executeWorkflow(
        <@ fun (wf: MyWorkflow) -> wf.run() @>,
        WorkflowOptions(ID = "fsharp-workflow-id", TaskQueue = "fsharp-sample")) |> Async.AwaitTask
    printfn $"Workflow result: {result}"
}

[<EntryPoint>]
let main args =
    match args with
    | [|"worker"|] -> runWorker () |> Async.RunSynchronously; 0
    | [|"worklow"|] -> executeWorkflow () |> Async.RunSynchronously; 0
    | _ -> failwith "Must provide single 'worker' or 'workflow' arg"
