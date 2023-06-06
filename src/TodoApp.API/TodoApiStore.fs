module TodoApp.TodoApi.Store

open System

open FSharp.Data.Sql

open TodoApp.Core.Interfaces
open TodoApp.Core.Types

open TodoApp.DataAccess.Database

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
        member this.getAllAsync() =
            async {
                let result =
                    query {
                        for row in context.Public.Todo do
                        sortBy row.CreatedAt
                    }
                    |> Seq.map TodoItem.ofRow
                    |> Seq.toList

                return result }
                    
        member this.cleanAsync() =
            async {
                let! result = 
                    query {
                        for row in context.Public.Todo do
                        where (row.CompletedOn <> None)
                    }
                    |> Seq.``delete all items from single table``
                    |> Async.AwaitTask
                    |> Async.Ignore

                return result }
                    
        member this.toggleAsync id =
            async {
                let theRow =
                    query {
                        for row in context.Public.Todo do
                        where (row.Id = id)
                        select row
                    }
                    |> Seq.tryExactlyOne

                let result =
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
                
                return result }
            
        member this.addAsync name =
            async {
                let newRow = context.Public.Todo.Create()
                newRow.Label <- name

                context.SubmitUpdates()

                let result =
                    {
                        Id = newRow.Id
                        Label = name
                    }
                    |> Incomplete
                return result }
        
        member this.getAsync id =
            async {
                let result =
                    query {
                        for row in context.Public.Todo do
                        where (row.Id = id)
                    }
                    |> Seq.tryExactlyOne
                    |> Option.map TodoItem.ofRow
                
                return result }

        member this.getByIndexAsync id =
            async {
                let allList =
                    query {
                        for row in context.Public.Todo do
                        sortBy row.CreatedAt
                    }
                    |> Seq.map TodoItem.ofRow
                    |> Seq.toList

                return allList |> List.tryItem id }
