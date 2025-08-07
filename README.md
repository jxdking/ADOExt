# ADOExt

## Description
A simple object mapping library. It uses ADO.NET at backend. The performance is on par with (if it is not faster) Entity Framework Core.


## Nuget
Oracle: [https://www.nuget.org/packages/MagicEastern.ADOExt.Oracle/](https://www.nuget.org/packages/MagicEastern.ADOExt.Oracle/)

Sql Server: [https://www.nuget.org/packages/MagicEastern.ADOExt.SqlServer/](https://www.nuget.org/packages/MagicEastern.ADOExt.SqlServer/)


## How to use
### Step 1
Put DBColumn attribute to the Properties of the class that you wish to map from database. Optionally, if you need to insert, update, delete, and load record from a table, put DBTable attribute to the class.
```c#
[DBTable("Department")]
public class DeptInfo
{
	[DBColumn] public string DEPT_ID { get; set; }
	[DBColumn] public string DESCRIPTION { get; set; }
}
```

### Step 2
Register your database services.
```c#
/* 
 * Register the DB Services. Thus, you can access DBServiceManager object
 * through dependency injection cross the application.
 * AddDatabase() can be called multiple times to add multiple databases.
 */
serviceCollection.AddDBServiceManger()
    .AddDatabase("mydb", () => new SqlConnection(SqlConnString));
```

### Step 3
Request DBServiceManager object from dependency injection, like the example below.
```c#
public SomeController(DBServiceManager dbManager)
```

### Step 4
Use the database.
```c#
using (var conn = dbManager.OpenConnection())
{
    var records = conn.Query<DeptInfo>("select * from Department");
}

/*
 * You may create transaction directly from DBServiceManager.
 * Using optional indexer on DBServiceManger object to
 * specify the database you want to use instead of the default one.
 * The default database will be the 1st database when you
 * calling AddDatabase() during registering services.
 */
using (var trans = dbManager["mydb"].BeginTransaction())
{
    trans.Insert(new DeptInfo
    {
	DEPT_ID = "IT",
	IODescriptionAttribute = "IT"
    });
    /*
     * If missing this line below, the transaction will be 
     * rolled back by default.
     */
    trans.Commit();
}
```

#### Extension Methods on the Connection object returned from OpenConnection():
```
Query<T>()
Execute()
GetSingleValue<T>
GetFirstColumn<T>
Load<T>
Insert<T>
Update<T>
Delete<T>
 ```
 See example in projects "Test"(SqlServer), "TestOracle"(Oracle).
