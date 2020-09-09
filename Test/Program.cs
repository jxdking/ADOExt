/*
 It use a sample databases "AdventureWords". 
 To install the database, please see: https://github.com/Microsoft/sql-server-samples/releases/tag/adventureworks
    */


using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MagicEastern.ADOExt;
using MagicEastern.ADOExt.SqlServer;
using Microsoft.Extensions.DependencyInjection;

namespace Test
{

    class Program
    {
        static IServiceProvider sp;

        static string SqlConnString = ConfigurationManager.ConnectionStrings["AdventureWorks.ConnectionString.SQL Server (SqlClient)"].ConnectionString;
        
        static void Main(string[] args)
        {
            IServiceCollection services = new ServiceCollection();
            services.AddADOExtSqlServer(() => new SqlConnection(SqlConnString));
            sp = services.BuildServiceProvider();
            TestSqlServer();
            Console.WriteLine("press any key to continue ...");
            Console.ReadKey();
        }


        static void TestSqlServer()
        {
            var rp = sp.GetService<ResolverProvider>();
            Console.WriteLine("*** Testing SqlServer Library...");
            using (var conn = rp.OpenConnection())
            {
                var trans = conn.BeginTransaction();

                var orders = Query(conn, trans);
                int cnt = GetSingleValue(conn, trans);
                var orderids = GetFirstColumn(conn, trans);
                SalesOrderHeader order = Load(conn, orderids.First(), trans);
                order = Insert(conn, trans);

                Update(conn, order, trans);
                cnt = Delete(conn, order, trans);

                trans.Rollback();
            }
        }

        static List<SalesOrderHeader> Query(DBConnectionWrapper conn, DBTransactionWrapper trans)
        {
            string sql = "SELECT [SalesOrderID],[RevisionNumber],[OrderDate],[DueDate],[ShipDate],[Status],[OnlineOrderFlag],[SalesOrderNumber],[PurchaseOrderNumber],[AccountNumber],[CustomerID],[SalesPersonID],[TerritoryID],[BillToAddressID],[ShipToAddressID],[ShipMethodID],[CreditCardID],[CreditCardApprovalCode],[CurrencyRateID],[SubTotal],[TaxAmt],[Freight],[TotalDue],[Comment],[rowguid],[ModifiedDate] FROM [Sales].[SalesOrderHeader]";
            var ret = conn.Query<SalesOrderHeader>(sql, trans);
            Console.WriteLine("Queried " + ret.Count + " records.");
            return ret;
        }

        static int GetSingleValue(DBConnectionWrapper conn, DBTransactionWrapper trans = null)
        {
            string sql = "SELECT count(*) FROM [Sales].[SalesOrderHeader]";
            var ret = conn.GetSingleValue<int>(sql, trans);
            Console.WriteLine("Total " + ret + " records in table.");
            return ret;
        }

        static List<int> GetFirstColumn(DBConnectionWrapper conn, DBTransactionWrapper trans = null)
        {
            string sql = "SELECT SalesOrderID FROM [Sales].[SalesOrderHeader]";
            var ret = conn.GetFirstColumn<int>(sql, trans);
            Console.WriteLine("Queried " + ret.Count + " lines of first column.");
            return ret;
        }

        static SalesOrderHeader Load(DBConnectionWrapper conn, int orderid, DBTransactionWrapper trans = null)
        {
            var ret = conn.Load(new SalesOrderHeader { SalesOrderId = orderid }, trans);
            Console.WriteLine("Sales Order Number of loaded record is " + ret.SalesOrderNumber);
            return ret;
        }

        static SalesOrderHeader Insert(DBConnectionWrapper conn, DBTransactionWrapper trans = null)
        {
            SalesOrderHeader order = new SalesOrderHeader
            {
                RevisionNumber = 8,
                OrderDate = DateTime.Today,
                DueDate = DateTime.Today.AddDays(7),
                Status = 1,
                OnlineOrderFlag = true,
                CustomerID = 25252,
                BillToAddressID = 20850,
                ShipToAddressID = 20850,
                ShipMethodID = 1,
                SubTotal = 100m,
                TaxAmt = 0m,
                Freight = 0m,
                Comment = "TEST",
                Rowguid = Guid.NewGuid(),
                ModifiedDate = DateTime.Now
            };
            int nor = conn.Insert(ref order, trans);
            var id = conn.GetSingleValue<int>("SELECT @@IDENTITY", trans);
            order.SalesOrderId = id;
            Console.WriteLine(nor + " record inserted.");
            return order;
        }

        static int Delete(DBConnectionWrapper conn, SalesOrderHeader obj, DBTransactionWrapper trans = null)
        {
            return conn.Delete(obj, trans);
        }

        static void Update(DBConnectionWrapper conn, SalesOrderHeader obj, DBTransactionWrapper trans = null)
        {
            int nor = conn.Update(ref obj, trans);
            Console.WriteLine(nor + " line updated.");
            var obj2 = new SalesOrderHeader();
            obj2.SalesOrderId = obj.SalesOrderId;
            obj2.ModifiedDate = DateTime.Now;
            obj2.Status = 2;
            nor = conn.Update(ref obj2, trans, i => i.ModifiedDate, i => i.Status);
            Console.WriteLine(nor + " line updated (only update ModifiedDate and Status).");
        }
    }
}
