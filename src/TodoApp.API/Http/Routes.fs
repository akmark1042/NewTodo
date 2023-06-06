module TodoApp.API.Http.Routes

open Microsoft.AspNetCore.Http

open Giraffe

open TodoApp.API.Http.Handlers
open TodoApp.API.Http.Auth

let webApp staticToken : HttpFunc -> HttpContext -> HttpFuncResult =
    choose [ subRoute
                 "/api"
                 (choose
                    [ staticBasic staticToken
                        >=>
                        subRouteCi "/v1/todos" //https://localhost:5001/api/v1/todos
                            (choose [
                                        GET >=> choose [
                                            route "" >=> handleListAll
                                            routeCif "/%O" handleGetOne
                                            routeCif "/index/%i" handleGetOneByIndex
                                        ]
                                        POST >=> routeCi "" >=> handleNewTodo
                                        PUT >=> routeCif "/index/%i/toggle" handleToggleByIndex
                                        DELETE >=> routeCi "/completed" >=> handleRemoveCompleted
                                    ]
                            )
                    ]
                 )
            ]