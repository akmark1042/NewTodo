module NewTodo.Program

open NewTodo.Store

open TodoApp.Core

open System

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

let rec todLoop (store: TodoCsvStore) loopToken =
    let command = Console.ReadLine()

    match command with
    | Help ->
        help store
        todLoop store loopToken
    | Add a ->
        add store loopToken a |> todLoop store
    | Get g ->
         get store loopToken g |> todLoop store
    | ListAll a ->
        listAll store loopToken |> todLoop store
    | Clean c ->
        clean store loopToken |> todLoop store
    | Toggle t ->
         toggle store loopToken t |> todLoop store
    | Exit e -> exit(0)
    | _ -> todLoop store loopToken

[<EntryPoint>]
let main args =
    printfn "Type an instruction: (such as help)"
    let store = new TodoCsvStore("..\\..\\..\\todoappstore.csv")
    help store
    todLoop store []
    0

    //dotnet tool install Migrondi https://github.com/AngelMunoz/Migrondi 
    /////////  ^^^ - make migrations - migrate? - create the DB and table?

    //Use PostGreSQL table to hold data instead of CSV
    
    //Push to github