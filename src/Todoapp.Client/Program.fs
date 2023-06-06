module TodoApp.Client.Program

open System
open Microsoft.Extensions.Logging

open TodoApp.Client.Types
open TodoApp.Client.ApiClient

let printTodoItem (idx:int) (item:TodoItem) =
    let (c, label) =
        match item with
        | Complete d -> ('x', d.Label)
        | Incomplete d -> (' ', d.Label)
    
    printfn " %i. [%c] %s" idx c label
 
let rec todoLoopAsync() =
    async {
        let command = Console.ReadLine()

        match command with
        | Help ->
            help()
            return! todoLoopAsync()
        | Add a ->
            do! addAsync a |> Async.Ignore
            return! todoLoopAsync()
        | Get g ->
            let! gotItem = getAsync g
            match gotItem with
            | Some i -> printTodoItem g i
            | None -> printfn "None"
            return! todoLoopAsync()
        | ListAll _ ->
            let! allItems = getAllAsync()
            allItems |> List.iteri printTodoItem
            return! todoLoopAsync()
        | Clean _ ->
            do! cleanAsync()
            return! todoLoopAsync()
        | Toggle t ->
            do! toggleAsync t |> Async.Ignore
            return! todoLoopAsync()
        | Exit e -> exit(0)
        | _ -> return! todoLoopAsync()
    }

[<EntryPoint>]
let main args =
    async {
        help()
        do! todoLoopAsync()
        return 0
    } |> Async.RunSynchronously
    