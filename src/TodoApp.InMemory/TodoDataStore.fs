module NewTodo.Factory

open TodoApp.Core.Interfaces
open TodoApp.Core.Types

//Implement new datastore in separate that calls CSVProvider; Paket
//Leave this one using the internal data store.

type TodoStore () = 
    interface ITodoStore with
        member this.clean loopToken =
            loopToken |> List.filter (fun c -> 
                match c with
                | Complete c -> false
                | Incomplete c -> true
            )
        
        member this.add loopToken name =
            List.append loopToken [ Incomplete {
                    Label = name
                }]

        member this.toggle loopToken num =
            let newList =
                match (List.tryItem num loopToken) with
                | Some item ->
                    match item with
                    | Incomplete a -> ([ Complete {
                            Label = a.Label
                            CompletedDate = System.DateTimeOffset.Now
                        }])
                    | Complete b -> ([ Incomplete {
                            Label = b.Label
                        }])
                | None -> loopToken
            
            try
                List.updateAt num newList.Head loopToken
            with
                | :? System.ArgumentException as ex -> printfn "No item found at this number. %s " (ex.Message); loopToken
                | :? System.InvalidOperationException as ex -> printfn "The list is empty. %s " (ex.Message); loopToken

        member this.listAll loopToken =
            loopToken |> List.iter (printfn "%A")
            loopToken
        
        member this.get loopToken num =
            List.tryItem num loopToken |> printfn "%A"
            loopToken
        
        member this.help() =
            printfn "Type an instruction: "
            printfn "Enter 'add' # (number) to add an item "
            printfn "Type 'exit' to end the program."