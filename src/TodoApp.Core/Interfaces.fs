[<AutoOpen>]
module TodoApp.Core.Interfaces

open System

open TodoApp.Core.Types

type ITodoStore =
    abstract clean: unit -> unit
    abstract add: string -> TodoItem
    abstract getAll: unit -> TodoItem list
    abstract get: Guid -> TodoItem option
    abstract toggle: Guid -> ToggleError option
    abstract getByIndex: int -> TodoItem option
    