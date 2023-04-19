module NewTodo.Store

open System

open FSharp.Data.Sql

open TodoApp.Core.Interfaces
open TodoApp.Core.Types

open NewTodo.Database

module TodoItem =
    let ofRow (row:TodoDb.dataContext.``public.todoEntity``) =
        match row.CompletedOn with
        | Some date ->
            {
                Id = row.Id
                Label = row.Label
                CompletedDate = DateTimeOffset(date)
            }
            |> Complete
        | None -> 
            {
                Id = row.Id
                Label = row.Label
            }
            |> Incomplete    

type TodoStore (context:TodoDb.dataContext) =
    interface ITodoStore with
        member this.getAll() =
            query {
                for row in context.Public.Todo do
                sortBy row.CreatedAt
            }
            |> Seq.map TodoItem.ofRow
            |> Seq.toList
                    
        member this.clean() =
            query {
                for row in context.Public.Todo do
                where (row.CompletedOn <> None)
            }
            |> Seq.``delete all items from single table``
            |> Async.AwaitTask
            |> Async.Ignore
            |> Async.RunSynchronously
                    
        member this.toggle id =
            let theRow =
                query {
                    for row in context.Public.Todo do
                    where (row.Id = id)
                    select row
                }
                |> Seq.tryExactlyOne

            match theRow with
            | Some item -> 
                match item.CompletedOn with
                | Some _ ->
                    item.CompletedOn <- None
                | None -> 
                    item.CompletedOn <- DateTime.UtcNow |> Some
                
                context.SubmitUpdates()
                None
            | None -> ToggleError.ItemNotFound id |> Some
            
        member this.add name =
            let newRow = context.Public.Todo.Create()
            newRow.Label <- name

            context.SubmitUpdates()

            {
                Id = newRow.Id
                Label = name
            }
            |> Incomplete
        
        member this.get id =
            query {
                for row in context.Public.Todo do
                where (row.Id = id)
            }
            |> Seq.tryExactlyOne
            |> Option.map TodoItem.ofRow

        member this.getByIndex id =
            let allList =
                query {
                    for row in context.Public.Todo do
                    sortBy row.CreatedAt
                }
                |> Seq.map TodoItem.ofRow
                |> Seq.toList

            allList |> List.tryItem id
