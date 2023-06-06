module TodoApp.Client.Types

open System

type JsonItem = {
        Id: Guid
        Label: string
        CompletedDate: DateTimeOffset Nullable
    }

type IncompleteItem = {
    Id: Guid
    Label: string
}

type CompleteItem = {
    Id: Guid
    Label: string
    CompletedDate: DateTimeOffset
}

type TodoItem =
    | Complete of CompleteItem
    | Incomplete of IncompleteItem

type NewItem = {
    Label: string
}

//Partial active patterns
let (|Help|_|) (command: string) =
    match command with
    | "help" -> () |> Some
    | _ -> None

let (|Clean|_|) (command: string) =
    match command with
    | "clean" -> true |> Some
    | _ -> None

let (|Exit|_|) (command: string) =
    match command with
    | "exit" -> true |> Some
    | _ -> None

let (|ListAll|_|) (command: string) =
    match command with
    | "list all" -> true |> Some
    | "listall" -> true |> Some
    | _ -> None

//Parsing function
let (|StartsWith|_|) (prefix: string) (command: string) =
    if command.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) then
        command.Substring(prefix.Length) |> Some
    else
        None

let (|Add|_|) (command: string) =
    match command with
    | StartsWith "add " x -> x |> Some
    | _ -> None

//Parse and handle error if someone types in alphabet
let (|Get|_|) (command: string) =
    match command with
    | StartsWith "get " s ->
        match Int32.TryParse s with
        | true, i -> Some i
        | false, _ -> None
    | _ -> None

let (|Toggle|_|) (command: string) =
    match command with
    | StartsWith "toggle" t ->
        match Int32.TryParse t with
        | true, i -> Some i
        | false, _ -> None
    | _ -> None

let help() =
    printfn "Type 'add' (name) to add an item."
    printfn "Type 'get' # (number) to retrieve an item."
    printfn "Type 'list all' to retrieve all existing items."
    printfn "Type 'clean' to remove completed Todo items."
    printfn "Type 'toggle' # to indicate item completion."
    printfn "Type 'exit' to end the program."
    printfn "Type an instruction: "