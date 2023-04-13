[<AutoOpen>]
module TodoApp.Core.Default

let clean (store: ITodoStore) loopToken =
     store.clean loopToken

let add (store: ITodoStore) loopToken name =
     store.add loopToken name

let listAll (store: ITodoStore) loopToken =
    loopToken

let get (store: ITodoStore) loopToken num =
     store.get loopToken num

let toggle (store: ITodoStore) loopToken num =
     store.toggle loopToken num

let help (store: ITodoStore) =
     store.help()