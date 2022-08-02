/*
 It use a sample databases "AdventureWords". 
 To install the database, please see: https://github.com/Microsoft/sql-server-samples/releases/tag/adventureworks
    */


using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
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
        static DBServiceManager DBManager;

        static readonly string SqlConnString = ConfigurationManager.ConnectionStrings["AdventureWorks.ConnectionString.SQL Server (SqlClient)"].ConnectionString;
        
        static void Main(string[] args)
        {
            var sc = new ServiceCollection();
            sc.AddDBServiceManger().AddDatabase("mydb", () => new SqlConnection(SqlConnString));
            var sp = sc.BuildServiceProvider();
            DBManager = sp.GetService<DBServiceManager>();
            TestSqlServer();
            Console.WriteLine("press any key to continue ...");
            Console.ReadKey();
        }


        static void TestSqlServer()
        {
            var rp = DBManager;
            Console.WriteLine("*** Testing SqlServer Library...");
            using (var conn = rp.OpenConnection())
            {
                var trans = conn.BeginTransaction();

                for (int i = 0; i < 1; i++) {
                    var orders = Query(conn, trans);
                }
                int cnt = GetSingleValue(conn, trans);
                var orderids = GetFirstColumn(conn, trans);
                SalesOrderHeader order = Load(conn, orderids.First(), trans);
                order = Load(conn, orderids.First(), trans);

                order = Insert(conn, trans);

                Update(conn, order, trans);
                cnt = Delete(conn, order, trans);

                trans.Rollback();
            }
        }

        static IEnumerable<SalesOrderHeader> Query(DBConnectionWrapper conn, DBTransactionWrapper trans)
        {
            Stopwatch sw = new Stopwatch();
            string sql = "SELECT [SalesOrderID],[RevisionNumber],[OrderDate],[DueDate],[ShipDate],[Status],[OnlineOrderFlag],[SalesOrderNumber],[PurchaseOrderNumber],[AccountNumber],[CustomerID],[SalesPersonID],[TerritoryID],[BillToAddressID],[ShipToAddressID],[ShipMethodID],[CreditCardID],[CreditCardApprovalCode],[CurrencyRateID],[SubTotal],[TaxAmt],[Freight],[TotalDue],[Comment],[rowguid],[ModifiedDate] FROM [Sales].[SalesOrderHeader]";
            var ds = conn.Query(sql, trans);
            sw.Start();
            var ret = conn.Query<SalesOrderHeader>(sql, trans).ToList();
            sw.Stop();
            Console.WriteLine($"Query {ret.Count} lines in {sw.ElapsedMilliseconds}ms");
            //var t = ret.Last();
            //t = ret.Take(2).Last();
            //var ret = conn.Query<SalesOrderHeader>(sql, trans).Take(1);
            //Console.WriteLine("Queried " + t.Count() + " records.");
            //return null;
            return ret;
        }

        static int GetSingleValue(DBConnectionWrapper conn, DBTransactionWrapper trans = null)
        {
            string sql = "SELECT count(*) FROM [Sales].[SalesOrderHeader]";
            var ret = conn.GetSingleValue<int>(sql, trans);
            Console.WriteLine("Total " + ret + " records in table.");
            return ret;
        }

        static IEnumerable<int> GetFirstColumn(DBConnectionWrapper conn, DBTransactionWrapper trans = null)
        {
            string sql = "SELECT SalesOrderID FROM [Sales].[SalesOrderHeader]";
            var ret = conn.GetFirstColumn<int>(sql, trans).ToList();
            Console.WriteLine("Queried " + ret.Count() + " lines of first column.");
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
            int nor = conn.Insert(order, null, trans);
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
            int nor = conn.Update(obj, trans: trans);
            Console.WriteLine(nor + " line updated.");
            var obj2 = new SalesOrderHeader();
            obj2.SalesOrderId = obj.SalesOrderId;
            obj2.ModifiedDate = DateTime.Now;
            obj2.Status = 2;
            nor = conn.Update(obj2, new { obj2.ModifiedDate, obj2.Status }, trans);
            Console.WriteLine(nor + " line updated (only update ModifiedDate and Status).");
        }
    }
}
