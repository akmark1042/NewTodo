module NewTodo.Store

open TodoApp.Core.Interfaces

open FSharp.Data

open System
open System.IO

[<Literal>]
let resolutionFolder = __SOURCE_DIRECTORY__

type Todos = CsvProvider<"..\\TodoApp.CSV\\template.csv", ResolutionFolder=resolutionFolder>

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
        member this.add unusedLoopToken name =            
            let myCsv = loadFile()
            let myCSVwithExtraRows = myCsv.Append [ Todos.Row(name, None) ]
            saveFile myCSVwithExtraRows
            unusedLoopToken
        
        member this.toggle unusedLoopToken num =
            let listAllCsv = loadFile()
            let gotItem = listAllCsv.Rows |> Seq.toList |> List.tryItem num
            printfn "%A" gotItem
            
            let newCsv =
                match gotItem with
                | Some item ->
                    match item.Label, item.Date with
                    | _, Some a -> Todos.Row(item.Label, None)
                    | _, _ -> Todos.Row(item.Label, Some DateTimeOffset.Now)
                | None -> gotItem.Value
            
            let appended = listAllCsv.Rows |> Seq.updateAt num newCsv |> (fun t -> new Todos(t))

            saveFile appended
            unusedLoopToken
            
        member this.listAll unusedLoopToken =
            let listAllCsv = loadFile()
            listAllCsv.Rows |> Seq.toList |> List.iter (fun row -> printfn "%A" row)
            unusedLoopToken
        
        member this.clean unusedLoopToken =
            let listToClean = loadFile().Rows |> Seq.filter (fun row -> row.Date = None) |> (fun t -> new Todos(t))
            saveFile listToClean

            unusedLoopToken

        member this.get unusedLoopToken num =
            let listAllCsv = loadFile()
            printfn "%A" (listAllCsv.Rows |> Seq.toList |> List.tryItem num)
            unusedLoopToken
        
        member this.help() =
            printfn "Type 'add' (name) to add an item."
            printfn "Type 'get' # (number) to retrieve an item."
            printfn "Type 'list all' to retrieve all existing items."
            printfn "Type 'clean' to remove completed Todo items."
            printfn "Type 'toggle' # to indicate item completion."
            printfn "Type 'exit' to end the program."
            printfn "Type an instruction: "