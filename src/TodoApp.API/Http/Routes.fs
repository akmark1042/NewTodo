module TodoApp.API.Http.Routes

open Microsoft.AspNetCore.Http

open Giraffe

open TodoApp.API.Http.Handlers

let webApp : HttpFunc -> HttpContext -> HttpFuncResult =
    subRouteCi "/api/v1/todos"
        (choose [
            GET >=> choose [ //Web browser
                routef "/toggle/%i" handleToggle
                route "/clean" >=> handleClean
                route "" >=> handleGetAll
                routef "/%i" handleGetOne
                routef "/%s" handleAdd
            ]
        ])