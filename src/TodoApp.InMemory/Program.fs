module NewTodo.Program

open System

open TodoApp.Core

open NewTodo.Store

let rec todLoop (store: ITodoStore) =
    let command = Console.ReadLine()

    match command with
    | Help ->
        help()
        todLoop store
    | Add a ->
        add store a |> ignore
        todLoop store
    | Get g ->
        get store g |> printfn "%A"
        todLoop store
    | ListAll a ->
        getAll store |> printfn "%A"
        todLoop store
    | Clean c ->
        clean store
        todLoop store
    | Toggle t ->
        toggle store t |> ignore
        todLoop store
    | Exit e -> exit(0)
    | _ -> todLoop store

[<EntryPoint>]
let main args =
    let store = new TodoStore()
    help()
    todLoop store
    0