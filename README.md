# ADOExt

## Description
A simple object mapping library. It gives you a couple useful extension methods for IDbConnection and IDbTransaction. It uses ADO.NET at backend. The performance is on par with Entity Framework Core.


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
Use the extension methods from IDbConnection and IDbTransaction to access the database.
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
