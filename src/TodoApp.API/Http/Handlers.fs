module TodoApp.API.Http.Handlers

open System
open Microsoft.Extensions.Primitives
open Microsoft.AspNetCore.Http

open FSharp.Control.TaskBuilder

open Giraffe

open TodoApp.Core
open TodoApp.API

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
        let! result = addAsync store body.Label

        match body.HasErrors() with
        | Some errors -> return! (RequestErrors.BAD_REQUEST errors next ctx)
        | _ ->
            ctx.SetStatusCode 201
            return! json result next ctx
    }

let handleGetOneByIndex (id:int) =
    fun (next: HttpFunc) (ctx: HttpContext) -> task {
        let store = ctx.GetService<ITodoStore>()

        let! mItem = getAsync store id
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
        let! result = getAllAsync store

        return! json (result |> Seq.map ReturnItem.ofTodoItem |> Seq.toList) next ctx
    }

let handleRemoveCompleted =
    fun (next: HttpFunc) (ctx: HttpContext) -> task {
        let store = ctx.GetService<ITodoStore>()

        do! cleanAsync store

        ctx.SetStatusCode 204
        return! next ctx
    }

let handleToggleByIndex (idx:int) =
    fun (next: HttpFunc) (ctx: HttpContext) -> task {
        let store = ctx.GetService<ITodoStore>()
        
        let! mError = toggleAsync store idx
        match mError with
        | None ->
            ctx.SetStatusCode 200
            return! next ctx
        | Some error ->
            let et =
                match error with
                | ToggleError.IndexNotFound i -> ErrorType.IndexNotFound i
                | ToggleError.ItemNotFound id -> ErrorType.ItemNotFound id
                
            let result = et |> ErrorType.toErrorResult
            result.Status |> ctx.SetStatusCode

            ctx.Response.Headers.ContentType <- StringValues "application/problem+json"

            return! json result next ctx
    }