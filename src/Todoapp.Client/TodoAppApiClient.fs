module TodoApp.Client.ApiClient

open System
open System.Net
open System.Net.Http
open System.Net.Http.Json
open System.Net.Http.Headers
open System.Text
open System.Text.Json

open Types

module TodoItem =
    let ofJsonItem (item:JsonItem) =
        match item.CompletedDate |> Option.ofNullable with
        | Some i ->
            {
                Id = item.Id
                Label = item.Label
                CompletedDate = i
            } |> Complete
        | None ->
            {
                Id = item.Id
                Label = item.Label
            } |> Incomplete

let getClient() =
    let result = new HttpClient()
    result.DefaultRequestHeaders.Authorization <- new AuthenticationHeaderValue("Basic", "stub")
    result.BaseAddress <- new Uri("http://localhost:5000/api/v1/")
    result

let getAsync (idx:int) : Async<TodoItem option> =
    async {
        use client = getClient()
        let! response = sprintf "todos/index/%i" idx |> client.GetAsync |> Async.AwaitTask
        
        if response.StatusCode = HttpStatusCode.NotFound then
            return None
        elif response.IsSuccessStatusCode |> not then
            return raise (sprintf "API returned unexpected status code: %O." response.StatusCode |> NotImplementedException)
        else
            let! result = response.Content.ReadFromJsonAsync<JsonItem>() |> Async.AwaitTask
            return result |> TodoItem.ofJsonItem |> Some
    }

let cleanAsync() =
    async {
        use client = getClient()
        let! response = client.DeleteAsync("todos/completed") |> Async.AwaitTask
        response.EnsureSuccessStatusCode() |> ignore
    }

let toggleAsync (idx:int) =
    async {
        use client = getClient()
        let! response = (sprintf "todos/index/%i/toggle" idx, null) |> client.PutAsync |> Async.AwaitTask

        response.EnsureSuccessStatusCode() |> ignore
    }

let getAllAsync() : Async<List<TodoItem>> =
    async {
        use client = getClient()
        let! response = client.GetAsync("todos") |> Async.AwaitTask
        
        if response.IsSuccessStatusCode |> not then
            return raise (sprintf "API returned unexpected status code: %O." response.StatusCode |> NotImplementedException)
        else
            let! result = response.Content.ReadFromJsonAsync<List<JsonItem>>() |> Async.AwaitTask
            return result |> List.map TodoItem.ofJsonItem
    }

let addAsync (name: string) =
    async {
        use client = getClient()

        let newItem = {
            Label = name
        }

        let! response = client.PostAsJsonAsync("todos", newItem) |> Async.AwaitTask
        
        response.EnsureSuccessStatusCode() |> ignore

        let! result = response.Content.ReadFromJsonAsync<IncompleteItem>() |> Async.AwaitTask

        return (result |> Incomplete)
    }

let getByIdAsync (id:Guid) : Async<JsonItem option> =
    async {
        use client = getClient()
        let! response = sprintf "todos/%O" id |> client.GetAsync |> Async.AwaitTask
        
        if response.StatusCode = HttpStatusCode.NotFound then
            return None
        elif response.IsSuccessStatusCode |> not then
            return raise (sprintf "API returned unexpected status code: %O." response.StatusCode |> NotImplementedException)
        else
            let! result = response.Content.ReadFromJsonAsync<JsonItem>() |> Async.AwaitTask
            return result |> Some
    }