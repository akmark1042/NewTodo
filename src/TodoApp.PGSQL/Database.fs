module NewTodo.Database

open FSharp.Data.Sql

[<Literal>]
let dbVendor = Common.DatabaseProviderTypes.POSTGRESQL

[<Literal>]
let connString = "Host=localhost;User ID=todo;Password=todo;Database=todo;"

[<Literal>]
let contextSchemaPath = __SOURCE_DIRECTORY__ + "/database.schema"

[<Literal>]
let useOptionTypes = Common.NullableColumnType.OPTION

type TodoDb = SqlDataProvider<
    DatabaseVendor = dbVendor,
    ConnectionString = connString,
    UseOptionTypes = useOptionTypes,
    ContextSchemaPath = contextSchemaPath >

// Uncomment to save schema
// let ctx = TodoDb.GetDataContext()
// ctx.``Design Time Commands``.SaveContextSchema