using MagicEastern.ADOExt;
using MagicEastern.ADOExt.Oracle.Core;
using Microsoft.Extensions.DependencyInjection;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;

namespace TestOracle.Core
{
    class Program
    {
        static IDBService db;

        static void Main(string[] args)
        {
            IServiceCollection services = new ServiceCollection();
            string connStr = ConfigurationManager.ConnectionStrings["OracleConnStr"].ConnectionString;
            services.AddOracle(() => new OracleConnection(connStr));
            var sp = services.BuildServiceProvider();
            db = sp.GetService<IDBService>();
            TestOracle();
            Console.WriteLine("press any key to continue ...");
            Console.ReadKey();
        }

        static void TestOracle()
        {
            Console.WriteLine("*** Testing Oracle Database ...");

            var rp = db;
            //Insert2(connStr);
            using (var conn = rp.OpenConnection())
            {
                var trans = conn.BeginTransaction();
                var recs = QueryOracle(conn, trans);
                int nor = GetSingleValue(conn, trans);
                var firstcol = GetFirstColumn(conn, trans);
                var emp = Load(conn, trans);
                var emp2 = Load(conn, trans);
                var inserted = Insert(conn, trans);
                //Update0(inserted, conn, trans);
                var updated = Update(conn, trans);
                nor = Delete(conn, trans);
                //sert2(conn, trans);

                trans.Rollback();
                conn.Close();
            }
        }

        static void Insert2(string connStr)
        {
            string sqltxt =
                @"begin
                insert into employees(EMPLOYEE_ID, LAST_NAME)
                values (207, :LAST_NAME) returning LAST_NAME into :LAST_NAME;
                end;";

            try
            {
                using (OracleConnection conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    var trans = conn.BeginTransaction();
                    var cmd = new OracleCommand(sqltxt, conn);
                    cmd.Transaction = trans;
                    cmd.BindByName = true;

                    var p1 = new OracleParameter("LAST_NAME", "Jin");
                    p1.Direction = ParameterDirection.InputOutput;
                    p1.Size = Int16.MaxValue;

                    cmd.Parameters.AddRange(new OracleParameter[] { p1 });
                    cmd.ExecuteNonQuery();

                    trans.Rollback();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine();

            try
            {
                var rp = db;
                using (var conn = rp.OpenConnection())
                {
                    var trans = conn.BeginTransaction();
                    Sql sql = new Sql(sqltxt, new OracleParameter { ParameterName = "LAST_NAME", Value = "Jin", Direction = ParameterDirection.InputOutput });
                    trans.Execute(sql, false);

                    trans.Rollback();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException.Message);
            }
        }

        static IEnumerable<Employee> QueryOracle(DBConnectionWrapper conn, DBTransactionWrapper trans)
        {
            //string sql = "select * from employees";

            Sql sql = new Sql("select * from employees where employee_id < :p_id").AddParamters(new { p_id = 110 });
            var ret = conn.Query<Employee>(sql, trans);
            Console.WriteLine(ret.Count() + " rows queried.");
            ret = conn.Query<Employee>(sql, trans);
            Console.WriteLine(ret.Count() + " rows queried.");
            return ret;
        }

        static int GetSingleValue(DBConnectionWrapper conn, DBTransactionWrapper trans)
        {
            string sql = "select count(*) from employees";
            var ret = conn.GetSingleValue<int>(sql, trans);
            Console.WriteLine(ret + " rows in employees table.");
            return ret;
        }


        static IEnumerable<string> GetFirstColumn(DBConnectionWrapper conn, DBTransactionWrapper trans)
        {
            string sql = "SELECT first_name FROM employees";
            var ret = conn.GetFirstColumn<string>(sql, trans);
            Console.WriteLine("Queried " + ret.Count() + " lines of first column.");
            return ret;
        }

        static Employee Load(DBConnectionWrapper conn, DBTransactionWrapper trans = null)
        {
            var ret = conn.Load(new Employee { EMPLOYEE_ID = 102 }, trans);
            Console.WriteLine("First name of loaded employee is " + ret.FIRST_NAME);
            return ret;
        }

        static Employee Insert(DBConnectionWrapper conn, DBTransactionWrapper trans = null)
        {
            Employee obj = new Employee
            {
                EMPLOYEE_ID = 207,
                FIRST_NAME = "Kevin",
                LAST_NAME = "Jin",
                EMAIL = "kevin.jin",
                PHONE_NUMBER = "123.456.7890",
                HIRE_DATE = DateTime.Now,
                JOB_ID = "AC_ACCOUNT",
                SALARY = 8300m,
                COMMISSION_PCT = null,
                MANAGER_ID = 205,
                DEPARTMENT_ID = 110
            };
            var ret = conn.Insert(obj, trans: trans);
            Console.WriteLine(ret + " rows inserted");
            return obj;
        }

        static Employee Update0(Employee obj, DBConnectionWrapper conn, DBTransactionWrapper trans = null)
        {
            obj.DEPARTMENT_ID = 60;
            obj.HIRE_DATE = DateTime.Now.AddDays(-2);
            conn.Update(obj, trans: trans);
            return obj;
        }

        static Employee Update(DBConnectionWrapper conn, DBTransactionWrapper trans = null)
        {
            Employee obj = new Employee
            {
                EMPLOYEE_ID = 207, // 207
                DEPARTMENT_ID = 50,
                HIRE_DATE = DateTime.Now.AddDays(-1)
            };
            int ret = trans.Update(obj, new { obj.HIRE_DATE }, out var res);
            //int ret = conn.Update(obj, new { obj.HIRE_DATE }, out var res, trans);
            Console.WriteLine(ret + " rows updated");
            if (ret > 0)
            {
                Console.WriteLine("The first name of the updated record is " + res.FIRST_NAME);
            }
            return obj;
        }

        static int Delete(DBConnectionWrapper conn, DBTransactionWrapper trans = null)
        {
            int ret = conn.Delete(new Employee { EMPLOYEE_ID = 207 }, trans);
            Console.WriteLine(ret + " rows deleted");
            return ret;
        }
    }
}
