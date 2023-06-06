module TodoApp.Api.Program

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration
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

type LocalWebHostBuilder =
    { Builder: IWebHostBuilder
      ConfigureFn: IApplicationBuilder -> IApplicationBuilder }

let withLocalBuilder builder = { Builder = builder; ConfigureFn = id }

let configureAppConfiguration fn (builder: LocalWebHostBuilder) =
    let bldr =
        builder.Builder.ConfigureAppConfiguration(
            Action<WebHostBuilderContext, IConfigurationBuilder>(fun ctx bldr -> fn ctx bldr |> ignore)
        )

    { builder with Builder = bldr }

let configureServices fn (builder: LocalWebHostBuilder) =
    let bldr =
        builder.Builder.ConfigureServices(
            Action<WebHostBuilderContext, IServiceCollection>(fun ctx svc -> fn ctx svc |> ignore)
        )

    { builder with Builder = bldr }

let configure (fn: IApplicationBuilder -> IApplicationBuilder) (builder: LocalWebHostBuilder) =
    let cfgFn = builder.ConfigureFn >> fn

    let bldr =
        builder.Builder.Configure(Action<IApplicationBuilder>(fun app -> cfgFn app |> ignore))

    { Builder = bldr; ConfigureFn = cfgFn }

let configureLogging fn (builder: LocalWebHostBuilder) =
    let bldr =
        builder.Builder.ConfigureLogging(
            Action<WebHostBuilderContext, ILoggingBuilder>(fun ctx bldr -> fn ctx bldr |> ignore)
        )

    { builder with Builder = bldr }


let build (bldr: IHostBuilder) = bldr.Build() 

let run (host: IHost) = host.Run()

// --------------------------------- 
// Config and Main
// ---------------------------------

let withConfiguration (bldr: LocalWebHostBuilder) =
    bldr
    |> configureAppConfiguration (fun context config ->
        config
            .AddJsonFile("appsettings.json", false, true)
            .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", true, true)
            .AddEnvironmentVariables()
        |> ignore)
let withSerilogRequestLogging (bldr: LocalWebHostBuilder) =
    bldr
    |> configure (fun app -> app.UseSerilogRequestLogging())

let withGiraffe bldr =
    bldr
    |> configureServices (fun _ services -> services.AddGiraffe())
    |> configure (fun app ->
        let config = app.ApplicationServices.GetService<IOptions<RootDbConfig>>().Value.NewTodo
        let env = app.ApplicationServices.GetService<IWebHostEnvironment>()

        if not (env.IsDevelopment()) then
            app.UseGiraffeErrorHandler(errorHandler) |> ignore

        app.UseGiraffe(webApp config.Token)
        app)

let withServices bldr =
    bldr |> configureServices (fun context services ->
        services
            .Configure<RootDbConfig>(context.Configuration)
            .AddScoped<TodoDb.dataContext>(fun provider ->
                let config = provider.GetRequiredService<IOptions<RootDbConfig>>()
                TodoDb.GetDataContext(config.Value.ConnectionStrings.TodoDb, selectOperations = SelectOperations.DatabaseSide)
            )
            .AddScoped<ITodoStore, TodoStore>()
            )

let configureSerilog (context: HostBuilderContext) (services: IServiceProvider) (config: LoggerConfiguration) =
    // template is default with addition of optional SourceContext (FromContext set Class) and optional Function (FromContext set Property)
    // note that only SourceContext or Function will be set, never both
    let logTemplate = "[{Timestamp:HH:mm:ss} {Level:u3} <{SourceContext}{Function}>] {Message:lj}{NewLine}{Exception}"
    config
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console(outputTemplate = logTemplate)
    |> ignore

///////////////////////////////////////////////////////////////
// For saving the schema
// let ctx = Database.GetDataContext()
// ctx.``Design Time Commands``.SaveContextSchema |> ignore
//
///////////////////////////////////////////////////////////////


[<EntryPoint>]
let main args =
    async {
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(fun webHostBuilder ->
                webHostBuilder
                |> withLocalBuilder
                |> withConfiguration
                |> withSerilogRequestLogging
                |> withGiraffe
                |> withServices
                |> ignore
                )
            .UseSerilog(configureSerilog)
            .Build()
            .Run()

        return 0
    } |> Async.RunSynchronously
