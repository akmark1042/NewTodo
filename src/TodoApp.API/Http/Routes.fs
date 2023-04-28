module TodoApp.API.Http.Routes

open Microsoft.AspNetCore.Http

open Giraffe

open TodoApp.API.Http.Handlers

let webApp : HttpFunc -> HttpContext -> HttpFuncResult =
    subRouteCi "/api/v1/todos"
        (choose [
            GET >=> choose [
                route "" >=> handleListAll
                routeCif "/%O" handleGetOne
                routeCif "/index/%i" handleGetOneByIndex
            ]
            POST >=> routeCi "" >=> handleNewTodo
            PUT >=> routeCif "/%O/toggle" handleToggleByGuid
            DELETE >=> routeCi "/completed" >=> handleRemoveCompleted
        ])
