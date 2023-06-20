namespace TemporalioSamples.FSharp

open Temporalio.Activities

type MyDatabaseClient () =
    member _.selectValue table =
        task { return $"some-db-value from table {table}" }

[<ReflectedDefinition>]
type MyActivities () =
    let dbClient = MyDatabaseClient()

    [<Activity>]
    static member doStaticThing () = task { return "some-static-value" }

    [<Activity>]
    member _.selectFromDatabase table =
        dbClient.selectValue table
