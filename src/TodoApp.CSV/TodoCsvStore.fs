module TodoApp.CSV.Store

open System
open System.IO

open FSharp.Data

open TodoApp.Core.Interfaces
open TodoApp.Core.Types

[<Literal>]
let resolutionFolder = __SOURCE_DIRECTORY__

type Todos = CsvProvider<"..\\TodoApp.CSV\\template.csv", ResolutionFolder=resolutionFolder>

//Type conversion
[<RequireQualifiedAccess>]
module TodoItem =
    let ofRow (row:Todos.Row) =
        match row.Date with
        | Some date ->
            {
                Id = row.Id
                Label = row.Label
                CompletedDate = date
            }
            |> Complete
        | None -> 
            {
                Id = row.Id
                Label = row.Label
            }
            |> Incomplete

type TodoCsvStore (path: string) =
    let saveFile (todos:Runtime.CsvFile<Todos.Row>) =
        use file = File.Open(path,FileMode.Create)
        todos.Save(file)

    let loadFile() =
        use file = File.Open(path,FileMode.OpenOrCreate)
        try
            Todos.Load(file)
        with
            | ex -> new Todos([])

    interface ITodoStore with
        member this.addAsync name =
            async {
                let result = 
                    let newToDoItem = Todos.Row(Guid.NewGuid(), name, None)
                    let myCSVwithExtraRows = loadFile().Append [ newToDoItem ]
                    saveFile myCSVwithExtraRows
                    newToDoItem |> TodoItem.ofRow
                
                return result }
        
        member this.toggleAsync id =
            async {
                let result =
                    let items = loadFile()

                    let mIndex =
                        items.Rows
                        |> Seq.tryFindIndex (fun i -> i.Id = id)
                    
                    match mIndex with
                    | None -> ToggleError.ItemNotFound id |> Some
                    | Some i ->
                        let item = items.Rows |> Seq.item i

                        let item' =
                            match item.Date with
                            | Some _ -> Todos.Row(id, item.Label, None)
                            | None -> Todos.Row(id, item.Label, Some DateTimeOffset.Now)

                        let appended = items.Rows |> Seq.updateAt i item' |> (fun t -> new Todos(t))

                        saveFile appended
                        None

                return result }

        member this.getAllAsync() =
            async {
                return loadFile().Rows
                |> Seq.toList
                |> List.map TodoItem.ofRow
            }
                    
        member this.cleanAsync() =
            async {
                let listToClean = loadFile().Rows |> Seq.filter (fun row -> row.Date = None) |> (fun t -> new Todos(t))
                saveFile listToClean
            }

        member this.getAsync id =
            async {
                return loadFile().Rows
                |> Seq.tryFind (fun row -> row.Id = id)
                |> Option.map TodoItem.ofRow
            }
                        
        member this.getByIndexAsync id =
            async {
                return loadFile().Rows
                |> Seq.tryItem id
                |> Option.map TodoItem.ofRow
            }
