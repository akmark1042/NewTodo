[<AutoOpen>]
module TodoApp.Core.Default

let cleanAsync (store: ITodoStore) =
     store.cleanAsync()

let addAsync (store: ITodoStore) name =
     store.addAsync name

let getAllAsync (store: ITodoStore)=
     store.getAllAsync()

let getAsync (store: ITodoStore) idx =
     store.getByIndexAsync idx

let toggleAsync (store: ITodoStore) idx =
     async {
          let! mItem = store.getByIndexAsync idx
          match mItem with
          | None -> return ToggleError.IndexNotFound idx |> Some
          | Some i ->
               return! i |> TodoItem.getId |> store.toggleAsync
     }
