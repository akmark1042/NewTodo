module TodoApp.Api.Program

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Options

open FSharp.Data.Sql

open Giraffe
open Serilog

open TodoApp.Core
open TodoApp.API.Http.Routes
open TodoApp.DataAccess.Database
open TodoApp.TodoApi.Store

// ---------------------------------
// Error handler
// ---------------------------------
let errorHandler (ex: Exception) (logger: Microsoft.Extensions.Logging.ILogger) =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")

    clearResponse
    >=> setStatusCode 500
    >=> text "An unhandled error occured."

// ---------------------------------
// Config Helpers
// ---------------------------------

let build (bldr: WebApplicationBuilder) =
    bldr.Build()

let run (app: WebApplication) =
    app.Run()

// --------------------------------- 
// Config and Main
// ---------------------------------

let configureSerilog
    (context : HostBuilderContext)
    (config: LoggerConfiguration)
    =
    config
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .WriteTo.Console()
    |> ignore

let addSerilog (bldr: WebApplicationBuilder): WebApplicationBuilder =
    configureSerilog |> bldr.Host.UseSerilog |> ignore
    bldr

let addServices (bldr: WebApplicationBuilder) =
    bldr.Services
        .Configure<RootDbConfig>(bldr.Configuration)
        .AddScoped<TodoDb.dataContext>(fun provider ->
            let config = provider.GetRequiredService<IOptions<RootDbConfig>>()
            TodoDb.GetDataContext(config.Value.ConnectionStrings.TodoDb, selectOperations = SelectOperations.DatabaseSide)
        )
        .AddScoped<ITodoStore, TodoStore>()
        |> ignore

    bldr

let useDevEnv (app: WebApplication) =
    let env = app.Services.GetService<IWebHostEnvironment>()
    if env.IsDevelopment() then
        app.UseDeveloperExceptionPage() |> ignore
    app

let addGiraffe (bldr: WebApplicationBuilder) =
    bldr.Services.AddGiraffe() |> ignore
    bldr

let useGiraffe (app: WebApplication) =
    let config = app.Services.GetService<IOptions<RootDbConfig>>().Value.NewTodo
    let staticToken = config.Token
    let env = app.Services.GetService<IWebHostEnvironment>()
    let logger = app.Services.GetService<ILogger>()
    app.UseGiraffe(webApp staticToken)
    if not (env.IsDevelopment()) then
        app.UseGiraffeErrorHandler(errorHandler) |> ignore
    app

let useSerilogRequestLogging (app: WebApplication) =
    app.UseSerilogRequestLogging() |> ignore
    app

///////////////////////////////////////////////////////////////
// For saving the schema
// let ctx = Database.GetDataContext()
// ctx.``Design Time Commands``.SaveContextSchema |> ignore
//
///////////////////////////////////////////////////////////////

[<EntryPoint>]
let main args =
    WebApplication.CreateBuilder(args)
        |> addSerilog
        |> addGiraffe
        |> addServices
        |> build
        |> useDevEnv
        |> useSerilogRequestLogging
        |> useGiraffe
        |> run
    
    0
