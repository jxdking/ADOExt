# ADOExt Documentation

## Description
ADOExt is a simple object-mapping library designed to simplify database interactions using ADO.NET at the backend. It aims to provide performance comparable to, or exceeding, Object-Relational Mappers like Entity Framework Core.

## NuGet Packages
*   **Oracle:** [https://www.nuget.org/packages/MagicEastern.ADOExt.Oracle/](https://www.nuget.org/packages/MagicEastern.ADOExt.Oracle/)
*   **SQL Server:** [https://www.nuget.org/packages/MagicEastern.ADOExt.SqlServer/](https://www.nuget.org/packages/MagicEastern.ADOExt.SqlServer/)

---

## How to Use ADOExt

### Step 1: Define Mappable Classes
Apply the `DBColumn` attribute to properties that map to database columns. If the class represents an entire table, apply the `DBTable` attribute.

**Example:**
```csharp
[DBTable("Department")]
public class DeptInfo
{
	[DBColumn] public string DEPT_ID { get; set; }
	[DBColumn] public string DESCRIPTION { get; set; }
}
```

### Step 2: Register Database Services
Register your database connections and services using dependency injection. The `AddDatabase()` method can be called multiple times to support multiple data sources.

**Example:**
```csharp
/* 
 * Registers the DB Service Manager, making it available for cross-application use.
 */
serviceCollection.AddDBServiceManager() // Corrected typo: Manger -> Manager
	.AddDatabase("mydb", () => new SqlConnection(SqlConnString));
```

### Step 3: Inject and Use the Database Manager
Request an instance of `DBServiceManager` via dependency injection in your controllers or services.

**Example:**
```csharp
public SomeController(DBServiceManager dbManager)
{
	// The service is now available for use.
}
```

### Step 4: Perform Database Operations
Use the injected manager to open connections, execute queries, and manage transactions.

#### Querying Data (Read Operation)
```csharp
using (var conn = dbManager.OpenConnection())
{
	// Executes a query and maps results to DeptInfo objects.
	var records = conn.Query<DeptInfo>("select * from Department");
}
```

#### Transactions (Write Operation)
Transactions can be managed directly via the `DBServiceManager` indexer, allowing you to specify which database context to use.

```csharp
using (var trans = dbManager["mydb"].BeginTransaction())
{
	trans.Insert(new DeptInfo
	{
		DEPT_ID = "IT",
		DESCRIPTION = "Information Technology" // Assuming IODescriptionAttribute was a typo for DESCRIPTION
	});
	/* 
	 * If trans.Commit() is omitted, the transaction will be rolled back by default.
	 */
	trans.Commit();
}
```

#### Connection Extension Methods
The `Connection` object returned from `OpenConnection()` provides several extension methods for common database operations:

*   `Query<T>()`: Executes a query and returns an enumerable of mapped objects.
*   `Execute()`: Executes a non-query command (e.g., stored procedure).
*   `GetSingleValue<T>()`: Retrieves a single scalar value.
*   `Load<T>()`, `Insert<T>()`, `Update<T>()`, `Delete<T>()`: Methods for CRUD operations on mapped objects.

> **Note:** See the "Test" (SqlServer) and "TestOracle" (Oracle) projects for detailed usage examples of these methods.
