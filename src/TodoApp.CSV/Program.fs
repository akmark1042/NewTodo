module NewTodo.Program

open System

open TodoApp.Core

open NewTodo.Store

let rec todoLoop (store: TodoCsvStore) =
    let command = Console.ReadLine()
    
    match command with
    | Help ->
        help()
        todoLoop store
    | Add a ->
        add store a |> ignore
        todoLoop store
    | Get g ->
        get store g |> printfn "%A"
        todoLoop store
    | ListAll a ->
        getAll store |> printfn "%A"
        todoLoop store
    | Clean c ->
        clean store
        todoLoop store
    | Toggle t ->
        toggle store t |> ignore
        todoLoop store
    | Exit e -> exit(0)
    | _ -> todoLoop store

[<EntryPoint>]
let main args =
    let store = new TodoCsvStore("todoappstore.csv")
    help()
    todoLoop store []
    0