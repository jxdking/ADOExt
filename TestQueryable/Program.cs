using MagicEastern.ADOExt;
using MagicEastern.ADOExt.IQueryable;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Testing.Platform.Configurations;
using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Text;
using MagicEastern.ADOExt.SqlServer;

namespace TestQueryable
{
    internal class Program
    {

        static Sql querysql = "SELECT [SalesOrderID],[RevisionNumber],[OrderDate],[DueDate],[ShipDate],[Status],[OnlineOrderFlag],[SalesOrderNumber],[PurchaseOrderNumber],[AccountNumber],[CustomerID],[SalesPersonID],[TerritoryID],[BillToAddressID],[ShipToAddressID],[ShipMethodID],[CreditCardID],[CreditCardApprovalCode],[CurrencyRateID],[SubTotal],[TaxAmt],[Freight],[TotalDue],[Comment],[rowguid],[ModifiedDate] FROM [Sales].[SalesOrderHeader]";

        static DBServiceManager DBManager;

        static readonly string SqlConnString = "data source=localhost;initial catalog=AdventureWorks2019;integrated security=SSPI;persist security info=False;packet size=4096";

        static void Main(string[] args)
        {
            var sc = new ServiceCollection();
            sc.AddDBServiceManger().AddDatabase("mydb", () => new System.Data.SqlClient.SqlConnection(SqlConnString));
            var sp = sc.BuildServiceProvider();
            DBManager = sp.GetService<DBServiceManager>();
            TestMethod1();
            Console.WriteLine("press any key to continue ...");
            Console.ReadKey();
        }



        public static void TestMethod1()
        {

            var search = new List<string> { "SO75121", "SO75120", "SO75119" };
            using (var conn = DBManager.OpenConnection()) {
                var ret = querysql.AsQueryable<SalesOrderHeader>(conn)
                    .Where(i => search.Contains(i.SalesOrderNumber))
                    .OrderByDescending(i => i.SalesOrderNumber);

                var sql = ret.ToSql();
                Console.WriteLine("done");
            }

            //List<string> list = ["aaa", "bbb", "cccc"];

            //var queryable = new SqlQueryable<string>(new SqlQueryProvider(list.AsQueryable()));

            //var res = queryable
            //    .Where(i => i == "aaa" || i == "bbb")
            //    .OrderBy(i => i)
            //    .ToList();

        }
    }
}
