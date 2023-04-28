[<AutoOpen>]
module TodoApp.API.Types

open System
open Giraffe

type NewTodo =
    {
        Label: string
    }

type ReturnItem = {
        Id: Guid
        Label: string
        CompletedDate: DateTimeOffset Nullable
    }
    // member this.HasErrors() =
    //         if this.Label.Length < 0 then Some "Invalid entry."
    //         else None

    // interface IModelValidation<ReturnItem> with
    //     member this.Validate() =
    //         match this.HasErrors() with
    //         | Some msg -> Error (RequestErrors.BAD_REQUEST msg)
    //         | None     -> Ok this