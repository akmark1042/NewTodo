module TodoApp.InMemory.Store

open System

open TodoApp.Core.Interfaces
open TodoApp.Core.Types

type TodoStore () =
    let mutable data = []
    interface ITodoStore with       
        member this.addAsync name =
            async {
                let newItem = List.append data [ Incomplete {
                        Id = Guid.NewGuid()
                        Label = name
                    }]
                data <- newItem
                return newItem.Head
            }

        member this.toggleAsync id =
            async {
                let result = 
                    let mIndex = (List.tryFindIndex (fun x -> TodoItem.getId x = id) data)
                    
                    match mIndex with
                    | None -> ToggleError.ItemNotFound id |> Some
                    | Some i ->
                        let item = List.tryItem i data

                        let newItem =
                            match item.Value with
                            | Incomplete a -> Complete {
                                    Id = Guid.NewGuid()
                                    Label = a.Label
                                    CompletedDate = DateTimeOffset.Now
                                }
                            | Complete b -> Incomplete {
                                    Id = Guid.NewGuid()
                                    Label = b.Label
                                }

                        data <- List.updateAt i newItem data
                        None
                
                return result
            }
        
        member this.getAllAsync() =
            async { return data }
        
        member this.getAsync id =
            async {
                return List.tryFind (fun x -> TodoItem.getId x = id) data
            }

        member this.getByIndexAsync id =
            async {
                return data |> List.tryItem id
            }
                    
        member this.cleanAsync() =
            async {
                data <- data |> List.filter (fun c -> 
                    match c with
                    | Complete c -> false
                    | Incomplete c -> true
                )
            }
           