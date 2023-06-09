﻿module TodoApp.InMemory.Program

open System

open TodoApp.Core

open TodoApp.InMemory.Store

let rec todoLoopAsync (store: ITodoStore) =
    async {
        let command = Console.ReadLine()

        match command with
        | Help ->
            help()
            return! todoLoopAsync store
        | Add a ->
            do! addAsync store a |> Async.Ignore
            return! todoLoopAsync store
        | Get g ->
            let! gotItem = getAsync store g
            printfn "%A" gotItem
            return! todoLoopAsync store
        | ListAll a ->
            let! allItems = getAllAsync store
            printfn "%A" allItems
            return! todoLoopAsync store
        | Clean c ->
            do! cleanAsync store
            return! todoLoopAsync store
        | Toggle t ->
            do! toggleAsync store t |> Async.Ignore
            return! todoLoopAsync store
        | Exit e -> exit(0)
        | _ -> return! todoLoopAsync store
    }

[<EntryPoint>]
let main args =
    async {
        let store = new TodoStore()
        help()
        do! todoLoopAsync store
        return 0
    } |> Async.RunSynchronously