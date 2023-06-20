module TemporalioSamples.FSharp.FSharpWorkflow

open System.Threading.Tasks
open Microsoft.FSharp.Quotations
open Temporalio.Activities
open Temporalio.Workflows

let executeTaskActivity (e: Expr<'a -> Task<'b>>, opts: ActivityOptions) =
    let (method, args) = FSharpExtensions.extractCall e
    Workflow.ExecuteActivityAsync<'b>(ActivityDefinition.NameFromMethod(method), args, opts)

let executeSyncActivity (e: Expr<'a -> 'b>, opts: ActivityOptions) =
    let (method, args) = FSharpExtensions.extractCall e
    Workflow.ExecuteActivityAsync<'b>(ActivityDefinition.NameFromMethod(method), args, opts)
