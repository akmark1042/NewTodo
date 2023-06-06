[<AutoOpen>]
module TodoApp.API.Types

open System
open Giraffe

type NewTodo =
    {
        Label: string
    }
    member this.HasErrors() =
        if this.Label.Length < 0 then Some "Invalid entry."
        else None

    interface IModelValidation<NewTodo> with
        member this.Validate() =
            match this.HasErrors() with
            | Some msg -> Error (RequestErrors.BAD_REQUEST msg)
            | None     -> Ok this

type ReturnItem = {
        Id: Guid
        Label: string
        CompletedDate: DateTimeOffset Nullable
    }

// RFC 7807
type ErrorResult = {
        Detail: string
        Type: string option
        Title: string
        Status: int
        Instance: string option
    }

type ErrorType =
    | IndexNotFound of int
    | ItemNotFound of Guid

module ErrorType =
    let toTypeString (et: ErrorType) =
        match et with
        | IndexNotFound _ -> None
        | ItemNotFound _ -> None
    
    let toTitle (et: ErrorType) =
        match et with
        | IndexNotFound _ -> "Item not found at index."
        | ItemNotFound _ -> "No item found with given ID."
    
    let toDetail (et: ErrorType) =
        match et with
        | IndexNotFound idx -> sprintf "Item not found at index: %i." idx
        | ItemNotFound id -> sprintf "No item found with given ID: %O." id

    let toStatus (et: ErrorType) =
        match et with
        | IndexNotFound _ -> 404
        | ItemNotFound _ -> 404
    
    let toErrorResult (et: ErrorType) : ErrorResult =
        {
            Detail = toDetail et
            Type = toTypeString et
            Title = toTitle et
            Status = toStatus et
            Instance = None
        }

type AuthToken = AuthToken of String

module AuthToken =
    let unwrap (AuthToken token) = token

    let validate (headers:string list) =
        match headers with
        | [] -> Error "No headers provided."
        | (value::_) -> Ok value
    
    let getToken (header:string) =
        if header.StartsWith("Basic ")
        then header.Replace("Basic ", "") |> Ok
        else Error "Not a basic authentication token."
    
    let make (authorization:string list) =
        authorization |> validate |> Result.bind getToken |> Result.map AuthToken