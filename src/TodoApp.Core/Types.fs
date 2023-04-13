module TodoApp.Core.Types

open System

type IncompleteItem = {
    Label: string
}

type CompleteItem = {
    Label: string
    CompletedDate: DateTimeOffset
}

type TodoItem =
    | Complete of CompleteItem
    | Incomplete of IncompleteItem