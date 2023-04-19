[<AutoOpen>]
module TodoApp.Core.Default

let clean (store: ITodoStore) =
     store.clean()

let add (store: ITodoStore) name =
     store.add name

let getAll (store: ITodoStore) =
    store.getAll()

let get (store: ITodoStore) idx =
     store.getByIndex idx

let toggle (store: ITodoStore) idx =
     let mItem = store.getByIndex idx
     match mItem with
     | None -> ToggleError.IndexNotFound idx |> Some
     | Some i ->
          i |> TodoItem.getId |> store.toggle
