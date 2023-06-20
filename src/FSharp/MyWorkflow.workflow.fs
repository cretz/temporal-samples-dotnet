namespace TemporalioSamples.FSharp

open System
open Microsoft.Extensions.Logging
open Temporalio.Workflows

[<Workflow>]
type MyWorkflow () =

    [<WorkflowRun>]
    member this.run () = task {
        let opts = ActivityOptions(StartToCloseTimeout = TimeSpan.FromMinutes 5)
        let executeActivity a = FSharpWorkflow.executeTaskActivity(a, opts)

        let! result1 =
            <@ fun (act: MyActivities) -> act.selectFromDatabase("some-db-table") @> |> executeActivity
        Workflow.Logger.LogInformation("Activity instance method result: {Result}", result1)

        let! result2 =
            <@ fun () -> MyActivities.doStaticThing() @> |> executeActivity
        Workflow.Logger.LogInformation("Activity static method result: {Result}", result2)

        // We'll go ahead and return this result
        return result2;
    }