namespace TemporalioSamples.Tests.FSharp

open TemporalioSamples.Tests
open TemporalioSamples.FSharp
open Xunit
open Xunit.Abstractions

type SomeMethods () =
    static member doStaticThing (s: string) = task { return s }
    
    member this.doInstanceThing (s: string) = task { return s }


type FSharpExtensionsTests (output: ITestOutputHelper) =
    inherit TestBase(output)

    [<Fact>]
    member _.``extractCall requires lambda`` () =
        let err = Assert.Throws(fun _ -> FSharpExtensions.extractCall <@ "foo" @> |> ignore)
        Assert.Contains("Lambda must be", err.Message)

    [<Fact>]
    member _.``extractCall static works`` () =
        let (method, args) =
            FSharpExtensions.extractCall(<@ fun () -> SomeMethods.doStaticThing "foo" @>)
        Assert.Equal("doStaticThing", method.Name)
        Assert.Equal<obj>(["foo"], args)

    [<Fact>]
    member _.``extractCall instance works`` () =
        let (method, args) =
            FSharpExtensions.extractCall(<@ fun (m: SomeMethods) -> m.doInstanceThing "foo" @>)
        Assert.Equal("doInstanceThing", method.Name)
        Assert.Equal<obj>(["foo"], args)
