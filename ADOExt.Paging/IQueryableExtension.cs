using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace MagicEastern.ADOExt.Paging
{
    public static class IQueryableExtension
    {
        public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, string propertyName, bool isDesc = false)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (propertyName == null) throw new ArgumentNullException("propertyName");

            Type type = typeof(T);
            ParameterExpression arg = Expression.Parameter(type, "x");

            PropertyInfo pi = type.GetProperty(propertyName);
            Type tKey = pi.PropertyType;
            Expression expr = Expression.Property(arg, pi);
            var lambda = Expression.Lambda(expr, arg);

            MethodInfo m;
            if (!isDesc)
            {
                m = typeof(Queryable).GetMethods().Where(i => i.Name == "TheyBy" && i.GetParameters().Count() == 2).First();
            }
            else
            {
                m = typeof(Queryable).GetMethods().Where(i => i.Name == "ThenByDescending" && i.GetParameters().Count() == 2).First();
            }

            return m.MakeGenericMethod(type, tKey).Invoke(source, new object[] { source, (Expression)lambda }) as IOrderedQueryable<T>;
        }

        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string propertyName, bool isDesc = false)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (propertyName == null) throw new ArgumentNullException("propertyName");

            Type type = typeof(T);
            ParameterExpression arg = Expression.Parameter(type, "x");

            PropertyInfo pi = type.GetProperty(propertyName);
            Type tKey = pi.PropertyType;
            Expression expr = Expression.Property(arg, pi);
            var lambda = Expression.Lambda(expr, arg);

            MethodInfo m;
            if (!isDesc)
            {
                m = typeof(Queryable).GetMethods().Where(i => i.Name == "OrderBy" && i.GetParameters().Count() == 2).First();
            }
            else
            {
                m = typeof(Queryable).GetMethods().Where(i => i.Name == "OrderByDescending" && i.GetParameters().Count() == 2).First();
            }

            return m.MakeGenericMethod(type, tKey).Invoke(source, new object[] { source, (Expression)lambda }) as IOrderedQueryable<T>;

            //return source.GetType().GetMethod("OrderBy<,>").MakeGenericMethod(type, tKey).Invoke(source, new object[] { (Expression)lambda }) as IOrderedQueryable<T>;
        }

        public static IQueryable<T> Paging<T>(this IQueryable<T> query, IPagingContext pc)
        {
            pc.TotalLines = query.Count();

            int skip = (pc.CurrentPage - 1) * pc.PageSize;
            if (pc.PageSize <= 0 || pc.CurrentPage <= 0 || pc.TotalLines <= skip)
            {
                return new List<T>().AsQueryable();
            }

            SortPara sp = pc.SortParaList.FirstOrDefault();

            IOrderedQueryable<T> oq;
            if (sp != null && typeof(T).GetProperty(sp.PropertyName, BindingFlags.Instance | BindingFlags.Public) != null)
            {
                oq = query.OrderBy(sp.PropertyName, sp.IsDesc);
            }
            else
            {
                oq = query.OrderBy(i => 0);
            }

            int take = pc.TotalLines - skip;
            if (take > pc.PageSize)
            {
                take = pc.PageSize;
            }

            return oq.Skip(skip).Take(take);
        }
    }
}