﻿/*
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
                conn.BeginTransaction();

                for (int i = 0; i < 10; i++) {
                    var orders = Query(conn);
                }
                int cnt = GetSingleValue(conn);
                var orderids = GetFirstColumn(conn);
                SalesOrderHeader order = Load(conn, orderids.First());
                order = Load(conn, orderids.Skip(1).First());

                order = Insert(conn);

                Update(conn, order);
                cnt = Delete(conn, order);

                conn.Rollback();
            }
        }

        static Sql querysql = "SELECT [SalesOrderID],[RevisionNumber],[OrderDate],[DueDate],[ShipDate],[Status],[OnlineOrderFlag],[SalesOrderNumber],[PurchaseOrderNumber],[AccountNumber],[CustomerID],[SalesPersonID],[TerritoryID],[BillToAddressID],[ShipToAddressID],[ShipMethodID],[CreditCardID],[CreditCardApprovalCode],[CurrencyRateID],[SubTotal],[TaxAmt],[Freight],[TotalDue],[Comment],[rowguid],[ModifiedDate] FROM [Sales].[SalesOrderHeader]";


        static IEnumerable<SalesOrderHeader> Query(DBConnectionWrapper conn)
        {
            Stopwatch sw = new Stopwatch();
            //var ds = conn.Query(querysql, trans);
            sw.Start();
            var ret = conn.Query<SalesOrderHeader>(querysql).ToList();
            sw.Stop();
            Console.WriteLine($"Query {ret.Count} lines in {sw.ElapsedMilliseconds}ms");
            //var t = ret.Last();
            //t = ret.Take(2).Last();
            //var ret = conn.Query<SalesOrderHeader>(sql, trans).Take(1);
            //Console.WriteLine("Queried " + t.Count() + " records.");
            //return null;
            return ret;
        }

        static int GetSingleValue(DBConnectionWrapper conn)
        {
            string sql = "SELECT count(*) FROM [Sales].[SalesOrderHeader]";
            var ret = conn.GetSingleValue<int>(sql);
            Console.WriteLine("Total " + ret + " records in table.");
            return ret;
        }

        static IEnumerable<int> GetFirstColumn(DBConnectionWrapper conn)
        {
            string sql = "SELECT SalesOrderID FROM [Sales].[SalesOrderHeader]";
            var ret = conn.GetFirstColumn<int>(sql).ToList();
            Console.WriteLine("Queried " + ret.Count() + " lines of first column.");
            return ret;
        }

        static SalesOrderHeader Load(DBConnectionWrapper conn, int orderid)
        {
            var ret = conn.Load(new SalesOrderHeader { SalesOrderId = orderid });
            Console.WriteLine("Sales Order Number of loaded record is " + ret.SalesOrderNumber);
            return ret;
        }

        static SalesOrderHeader Insert(DBConnectionWrapper conn)
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
            
            var sql = conn.DBService.GetInsertSql<SalesOrderHeader>(order, null, "_1");
            int nor = conn.Execute(sql, false);

            //int nor = conn.Insert(order, null, trans);
            var id = conn.GetSingleValue<int>("SELECT @@IDENTITY");
            order.SalesOrderId = id;
            Console.WriteLine(nor + " record inserted.");
            return order;
        }

        static int Delete(DBConnectionWrapper conn, SalesOrderHeader obj)
        {
            return conn.Delete(obj);
        }

        static void Update(DBConnectionWrapper conn, SalesOrderHeader obj)
        {
            int nor = conn.Update(obj);
            Console.WriteLine(nor + " line updated.");
            var obj2 = new SalesOrderHeader();
            obj2.SalesOrderId = obj.SalesOrderId;
            obj2.ModifiedDate = DateTime.Now;
            obj2.Status = 2;
            nor = conn.Update(obj2, new { obj2.ModifiedDate, obj2.Status });
            Console.WriteLine(nor + " line updated (only update ModifiedDate and Status).");
        }
    }
}
