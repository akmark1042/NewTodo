module NewTodo.Program

open System

open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Options

open TodoApp.Core

open NewTodo.Database
open NewTodo.Store

open FSharp.Data.Sql

// --------------------------------- 
// Config Helpers 
// ---------------------------------

let configureAppConfiguration fn (builder: IHostBuilder) = 
    builder.ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> (fun ctx bldr -> fn ctx bldr |> ignore)) 

let configureServices fn (builder: IHostBuilder) = 
    builder.ConfigureServices(Action<HostBuilderContext, IServiceCollection> (fun ctx svc -> fn ctx svc |> ignore)) 

//let configureLogging fn (builder: IHostBuilder) = 
//    builder.ConfigureLogging(Action<HostBuilderContext, ILoggingBuilder> (fun ctx bldr -> fn ctx bldr |> ignore)) 

let build (bldr: IHostBuilder) = bldr.Build() 

let run (host: IHost) = host.Run()

// --------------------------------- 
// Config and Main
// ---------------------------------

let withServices bldr =
    bldr |> configureServices (fun context services ->
        services
            .Configure<RootConfig>(context.Configuration)
            .AddScoped<TodoDb.dataContext>(fun provider ->
                let config = provider.GetRequiredService<IOptions<RootConfig>>()
                TodoDb.GetDataContext(config.Value.ConnectionStrings.TodoDb, selectOperations = SelectOperations.DatabaseSide)
            )
            .AddScoped<ITodoStore,TodoStore>()
    )

let rec todLoop (store: ITodoStore) =
    let command = Console.ReadLine()

    match command with
    | Help ->
        help()
        todLoop store
    | Add a ->
        add store a |> ignore
        todLoop store
    | Get g ->
        get store g |> printfn "%A"
        todLoop store
    | ListAll a ->
        getAll store |> printfn "%A"
        todLoop store
    | Clean _ ->
        clean store
        todLoop store
    | Toggle t ->
        toggle store t |> ignore
        todLoop store
    | Exit _ -> exit(0)
    | _ -> todLoop store

[<EntryPoint>]
let main args =
    let host =
        Host.CreateDefaultBuilder(args)
        |> withServices
        |> build

    let store = host.Services.GetRequiredService<ITodoStore>()
    help()
    todLoop store

    0
