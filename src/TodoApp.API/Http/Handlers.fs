module TodoApp.API.Http.Handlers

open Microsoft.AspNetCore.Http

open FSharp.Control.TaskBuilder
open Giraffe

open TodoApp.Core
open TodoApp.API
open System

module ReturnItem =
    let ofTodoItem (item:TodoItem) =
        match item with
        | Complete comp -> {
                Id = comp.Id
                Label = comp.Label
                CompletedDate = Nullable(comp.CompletedDate)
            }
        | Incomplete comp -> {
                Id = comp.Id
                Label = comp.Label
                CompletedDate = Nullable()
            }

let handleGetOne (id:Guid) =
    fun (next: HttpFunc) (ctx: HttpContext) -> task {
        let store = ctx.GetService<ITodoStore>()
        
        let! mItem = store.getAsync id
        match mItem with
        | None ->
            ctx.SetStatusCode 404
            return! next ctx
        | Some i ->
            ctx.SetStatusCode 200
            return! json (i |> ReturnItem.ofTodoItem) next ctx
    }

let handleNewTodo =
    fun (next: HttpFunc) (ctx: HttpContext) -> task {
        let store = ctx.GetService<ITodoStore>()
        let! body = ctx.BindJsonAsync<NewTodo>()
        let! result = store.addAsync body.Label
        
        //error handling for incorrect data passed intoo the body
        //Use Giraffe Model validation

        //ctx.Response.Headers.Location ""
        ctx.SetStatusCode 201
        return! json result next ctx
    }

let handleGetOneByIndex (id:int) =
    fun (next: HttpFunc) (ctx: HttpContext) -> task {
        let store = ctx.GetService<ITodoStore>()

        let! mItem = store.getByIndexAsync id
        match mItem with
        | None ->
            ctx.SetStatusCode 404
            return! next ctx
        | Some i ->
            ctx.SetStatusCode 200
            return! json (i |> ReturnItem.ofTodoItem) next ctx
    }

let handleListAll =
    fun (next: HttpFunc) (ctx: HttpContext) -> task {
        let store = ctx.GetService<ITodoStore>()
        let! result = store.getAllAsync()

        return! json (result |> Seq.map ReturnItem.ofTodoItem |> Seq.toList) next ctx
    }

let handleRemoveCompleted =
    fun (next: HttpFunc) (ctx: HttpContext) -> task {
        let store = ctx.GetService<ITodoStore>()
        do! store.cleanAsync()

        ctx.SetStatusCode 204
        return! next ctx
    }

let handleToggleByGuid (id:Guid) =
    fun (next: HttpFunc) (ctx: HttpContext) -> task {
        let store = ctx.GetService<ITodoStore>()
        
        let! mItem = store.getAsync id
        match mItem with
        | None ->
            ctx.SetStatusCode 404
            return! next ctx
        | Some todoItem ->
            do! todoItem |> TodoItem.getId |> store.toggleAsync |> Async.Ignore
            let opnTogErr = todoItem |> TodoItem.getId |> store.toggleAsync
            ctx.SetStatusCode 200
            return! json opnTogErr next ctx
    }