namespace TemporalioSamples.Tests.FSharp

open Temporalio.Client
open Temporalio.Testing
open Temporalio.Worker
open TemporalioSamples.Tests
open TemporalioSamples.FSharp
open TemporalioSamples.FSharp.FSharpExtensions
open Xunit
open Xunit.Abstractions

type MyWorkflowTests (output: ITestOutputHelper) =
    inherit TestBase(output)

    [<Fact>]
    member _.``Running workflow works`` () = task {
        // Start time-skipping server
        use! env = WorkflowEnvironment.StartTimeSkippingAsync ()

        // Prepare worker
        let activities = MyActivities ()
        let opts = TemporalWorkerOptions(TaskQueue = "fsharp-sample")
        // TODO(cretz): This approach of adding an activity is not gonna work, consider quotations
        opts.addActivity(activities.selectFromDatabase)
        opts.addActivity(MyActivities.doStaticThing)
        opts.addWorkflow<MyWorkflow>()

        // Run worker
        use worker = new TemporalWorker(env.Client, opts)
        return! worker.ExecuteAsync(fun () -> task {
            let! result = env.Client.executeWorkflow(
                <@ fun (wf: MyWorkflow) -> wf.run() @>,
                WorkflowOptions(ID = "fsharp-workflow-id", TaskQueue = "fsharp-sample"))
            Assert.Equal("some-static-value", result)
        })
    }

