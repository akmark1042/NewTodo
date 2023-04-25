module TodoApp.API.Http.Handlers

open Microsoft.AspNetCore.Http

open FSharp.Control.TaskBuilder
open Giraffe

open TodoApp.Core

let handleGetAll =
    fun (next: HttpFunc) (ctx: HttpContext) -> task {
        let store = ctx.GetService<ITodoStore>()
        let! result = store.getAllAsync()
        
        return! json result next ctx
    }

let handleGetOne id =
    fun (next: HttpFunc) (ctx: HttpContext) -> task {
        let store = ctx.GetService<ITodoStore>()
        let! result = store.getByIndexAsync id
        
        return! json result next ctx
    }

let handleAdd item =
    fun (next: HttpFunc) (ctx: HttpContext) -> task {
        let store = ctx.GetService<ITodoStore>()
        let! result = store.addAsync item
        
        return! json result next ctx
    }

let handleClean =
    fun (next: HttpFunc) (ctx: HttpContext) -> task {
        let store = ctx.GetService<ITodoStore>()
        let! result = store.cleanAsync()
        
        return! json result next ctx
    }

let handleToggle id =
    fun (next: HttpFunc) (ctx: HttpContext) -> task {
        let store = ctx.GetService<ITodoStore>()

        let! mItem = store.getByIndexAsync id
        match mItem with
        | None -> return! json (ToggleError.IndexNotFound id |> Some) next ctx 
        | Some i ->
            let! itmToggle = i |> TodoItem.getId |> store.toggleAsync
            return! json itmToggle next ctx
    }