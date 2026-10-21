using MagicEastern.ADOExt.IQueryable;
using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Text;

namespace TestQueryable
{
    internal class Program
    {
        class Car
        {
            public string BRAND { get; set; } = "";
            public string MODEL { get; set; } = "";

        }


        static void Main(string[] args)
        {
            TestMethod1();
        }

        public static void TestMethod1()
        {
            List<Car> list = [
                new Car{ BRAND = "Honda", MODEL = "Civic" },
                new Car{ BRAND = "Toyota", MODEL = "Corolla" }
            ];

            var exp = list.AsQueryable().Where(i => i.BRAND == "Honda").OrderBy(i => i.MODEL);

            var provider = new SqlExpressionVisitor();
            var ret = provider.GetSqlQuery(exp.Expression);

            Console.WriteLine(ret);

            //List<string> list = ["aaa", "bbb", "cccc"];

            //var queryable = new SqlQueryable<string>(new SqlQueryProvider(list.AsQueryable()));

            //var res = queryable
            //    .Where(i => i == "aaa" || i == "bbb")
            //    .OrderBy(i => i)
            //    .ToList();

        }
    }
}
