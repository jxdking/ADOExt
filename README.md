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
Create a ResolverProvider object. It will be used to create DBConnectionWrapper object. You can cache it static somewhere for later use throughout the application. 
```c#
var rp = ResolverProvider(() => new SqlConnection(SqlConnString))
```

### Step 3
Create a DBConnectionWrapper object when you want to access database.
```c#
using (DBConnectionWrapper conn = rp.OpenConnection())
{
	// use conn to access database.
}
```
DBConnectionWrapper class extends IDbConnection interface. Thus, you can use this object as usual SqlConneciton or OracleConnection object. There are also some useful extension methods on DBConnectionWrapper.
#### Extension Methods:
```
Query<T>
Execute
GetSingleValue<T>
GetFirstColumn<T>
Load<T>
Insert<T>
Update<T>
Delete<T>
 ```
 See example in projects "Test"(SqlServer), "TestOracle"(Oracle).
