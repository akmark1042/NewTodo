[<AutoOpen>]
module TodoApp.Core.Interfaces

open System

open TodoApp.Core.Types

type ITodoStore =
    abstract cleanAsync: unit -> Async<unit>
    abstract addAsync: string -> Async<TodoItem>
    abstract getAllAsync: unit -> Async<TodoItem list>
    abstract getAsync: Guid -> Async<TodoItem option>
    abstract toggleAsync: Guid -> Async<ToggleError option>
    abstract getByIndexAsync: int -> Async<TodoItem option>
