module TodoApp.Client.Store

open System
open System.Net.Http
open System.Net.Http.Json
open System.Text
open System.Text.Json

open FSharp.Data.Sql

open TodoApp.Core.Interfaces
open TodoApp.Core.Types
open TodoApp.API

module TodoItem =
    let ofReturnItem (item:ReturnItem) : (TodoItem) =
        let itemSwitch = 
            match item.CompletedDate.HasValue with
            | true -> Complete {
                    Id = item.Id
                    Label = item.Label
                    CompletedDate = item.CompletedDate.Value
                }
            | false -> Incomplete {
                    Id = item.Id
                    Label = item.Label
                }
        itemSwitch

type TodoStore (url: string) =
    let httpClient = new HttpClient()
    interface ITodoStore with
        member this.getAllAsync() =
            let wholeList() = async {
                let! response = httpClient.GetFromJsonAsync<list<ReturnItem>>(url) |> Async.AwaitTask
                return (response |> List.map TodoItem.ofReturnItem)
            }

            wholeList()

        member this.addAsync (name:string) =
            let newAdd() = async {
                let ret = {
                        Id = Guid.NewGuid()
                        Label = name
                    }

                let json = JsonSerializer.Serialize(ret)
                use content = new StringContent(json, Encoding.UTF8, "application/json")
                httpClient.PostAsync(url, content) |> Async.AwaitTask |> ignore

                return (ret |> Incomplete)}
            
            newAdd()

        member this.toggleAsync (id:Guid) : Async<option<ToggleError>>=
            let toggled() = async {
                httpClient.PutAsJsonAsync((url + "/" + id.ToString() + "/toggle"), "{}") |> Async.AwaitTask |> ignore
                return ToggleError.ItemNotFound id |> Some                
            }

            toggled()
                
        member this.getAsync (id:Guid) =
            let byId() = async {
                let! response = httpClient.GetFromJsonAsync<ReturnItem>(url) |> Async.AwaitTask

                return Some (response |> TodoItem.ofReturnItem)
            }

            byId()

        member this.getByIndexAsync idx =
            let byIndex() = async {
                let innerURL = url + "/index/" + idx.ToString()
                let! statCode = httpClient.GetAsync(innerURL) |> Async.AwaitTask

                match statCode.IsSuccessStatusCode with
                | false -> return None
                | true ->
                    let! response = httpClient.GetFromJsonAsync<ReturnItem>(innerURL) |> Async.AwaitTask
                    return Some (response |> TodoItem.ofReturnItem)
            }
            
            byIndex()
        
        member this.cleanAsync() =
            let cleanedList() = async {
                httpClient.DeleteAsync(url + "/completed") |> Async.AwaitTask |> ignore
                ()
                }

            cleanedList()
            