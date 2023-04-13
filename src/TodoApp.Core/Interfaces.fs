[<AutoOpen>]
module TodoApp.Core.Interfaces

open TodoApp.Core.Types

type ITodoStore =
    abstract clean: List<TodoItem> -> List<TodoItem>
    abstract add: List<TodoItem> -> string -> List<TodoItem>
    abstract listAll: List<TodoItem> -> List<TodoItem>
    abstract get: List<TodoItem> -> int -> List<TodoItem>
    abstract toggle: List<TodoItem> -> int -> List<TodoItem>
    abstract help: unit -> unit