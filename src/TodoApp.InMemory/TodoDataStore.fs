module TodoApp.InMemory.Store

open System

open TodoApp.Core.Interfaces
open TodoApp.Core.Types

type TodoStore () =
    let mutable data = []
    interface ITodoStore with       
        member this.addAsync name =
            let newAdd() = async {
                let newItem = List.append data [ Incomplete {
                        Id = Guid.NewGuid()
                        Label = name
                    }]
                data <- newItem
                return newItem.Head }
            
            newAdd()

        member this.toggleAsync id =
            let toggled() = async {
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

            toggled()
        
        member this.getAllAsync() =
            let wholeList() = async {
                return data
            }

            wholeList()
        
        member this.getAsync id =
            let byId() = async {
                return List.tryFind (fun x -> TodoItem.getId x = id) data
            }

            byId()

        member this.getByIndexAsync id =
            let byIndex() = async {
                return data |> List.tryItem id
            }
            
            byIndex()
        
        member this.cleanAsync() =
            let cleanedList() = async {
                data <- data |> List.filter (fun c -> 
                    match c with
                    | Complete c -> false
                    | Incomplete c -> true
                )}

            cleanedList()
            