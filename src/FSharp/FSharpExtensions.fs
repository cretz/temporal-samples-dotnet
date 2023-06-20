module TemporalioSamples.FSharp.FSharpExtensions

open System
open System.Threading.Tasks
open Microsoft.FSharp.Linq.RuntimeHelpers
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns
open Temporalio.Client
open Temporalio.Workflows

let extractCall (e: Expr) =
    match e with
    // Static
    | Lambda(v, Call(None, method, exprs)) when v.Type = typedefof<Unit> ->
        (method, exprs |> List.map LeafExpressionConverter.EvaluateQuotation)
    // Instance
    | Lambda(v, Call(Some(Var(inst)), method, exprs)) when v = inst ->
        (method, exprs |> List.map LeafExpressionConverter.EvaluateQuotation)
    | _ -> failwith "Lambda must be unit param + static call or single param + instance call on that param"

type Temporalio.Worker.TemporalWorkerOptions with
    member this.addActivity (a: 'a -> 'b) =
        
        this.AddActivity(new Func<'a, 'b>(a)) |> ignore

    member this.addWorkflow<'a> () =
        this.AddWorkflow<'a>() |> ignore

type Temporalio.Client.ITemporalClient with
    member this.executeWorkflow (e: Expr<'a -> Task<'b>>, opts: WorkflowOptions) =
        let (method, args) = extractCall(e)
        this.ExecuteWorkflowAsync<'b>(WorkflowDefinition.FromRunMethod(method).Name, args, opts)