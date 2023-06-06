module TodoApp.PGSQL.Program

open System

open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Options

open TodoApp.Core

open TodoApp.DataAccess.Database
open TodoApp.PGSQL.Store

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
            .Configure<RootDbConfig>(context.Configuration)
            .AddScoped<TodoDb.dataContext>(fun provider ->
                let config = provider.GetRequiredService<IOptions<RootDbConfig>>()
                TodoDb.GetDataContext(config.Value.ConnectionStrings.TodoDb, selectOperations = SelectOperations.DatabaseSide)
            )
            .AddScoped<ITodoStore,TodoStore>()
    )

let rec todoLoopAsync (store: ITodoStore) =
    async {
        let command = Console.ReadLine()

        match command with
        | Help ->
            help()
            return! todoLoopAsync store
        | Add a ->
            do! addAsync store a |> Async.Ignore
            return! todoLoopAsync store
        | Get g ->
            let! retItem = getAsync store g
            printfn "%A" retItem
            return! todoLoopAsync store
        | ListAll a ->
            let! retItem = getAllAsync store
            printfn "%A" retItem
            return! todoLoopAsync store
        | Clean _ ->
            do! cleanAsync store
            return! todoLoopAsync store
        | Toggle t ->
            do! toggleAsync store t |> Async.Ignore
            return! todoLoopAsync store
        | Exit _ -> exit(0)
        | _ -> return! todoLoopAsync store
    }

[<EntryPoint>]
let main args =
    async {
        let host =
            Host.CreateDefaultBuilder(args)
            |> withServices
            |> build

        let store = host.Services.GetRequiredService<ITodoStore>()
        help()
        do! todoLoopAsync store

        return 0
    } |> Async.RunSynchronously
