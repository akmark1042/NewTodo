module TodoApp.Api.Program

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Options

open Giraffe
open Serilog

open TodoApp.Core
open TodoApp.API.Http.Routes
open TodoApp.DataAccess.Database
open TodoApp.TodoDb.Store
open FSharp.Data.Sql

// ---------------------------------
// Error handler
// ---------------------------------
let errorHandler (ex : Exception) (logger : Microsoft.Extensions.Logging.ILogger) =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text "An unhandled error occured."

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

let addLogging (bldr: WebApplicationBuilder) =
    bldr.Logging
        .AddConsole()
        .AddDebug()
        |> ignore
    bldr

let addSerilog (bldr: WebApplicationBuilder): WebApplicationBuilder =
    configureSerilog |> bldr.Host.UseSerilog |> ignore
    bldr |> addLogging

let addGiraffe (bldr: WebApplicationBuilder) =
    bldr.Services.AddGiraffe() |> ignore
    bldr

let useGiraffe (app: WebApplication) =
    let env = app.Services.GetService<IWebHostEnvironment>()
    app.UseGiraffe(webApp)
    if not (env.IsDevelopment()) then
        app.UseGiraffeErrorHandler(errorHandler) |> ignore
    app

let addCors (bldr: WebApplicationBuilder) =
    bldr.Services.AddCors() |> ignore
    bldr

let configureCors (bldr : CorsPolicyBuilder) =
    bldr
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader()
        |> ignore

let useCors (app: WebApplication) =
    app.UseCors(configureCors) |> ignore
    app

let addServices (bldr: WebApplicationBuilder) =
    bldr.Services
        .Configure<RootConfig>(bldr.Configuration)
        .AddScoped<TodoDb.dataContext>(fun provider ->
            let config = provider.GetRequiredService<IOptions<RootConfig>>()
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

let useSerilogRequestLogging (app: WebApplication) =
    app.UseSerilogRequestLogging() |> ignore
    app

[<EntryPoint>]
let main args =
    WebApplication.CreateBuilder(args)
        |> addSerilog
        |> addCors
        |> addGiraffe
        |> addServices
        |> build
        |> useDevEnv
        |> useCors
        |> useSerilogRequestLogging
        |> useGiraffe
        |> run
    0
