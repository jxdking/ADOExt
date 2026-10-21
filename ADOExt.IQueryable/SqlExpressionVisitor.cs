using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MagicEastern.ADOExt.IQueryable
{
    public class SqlExpressionVisitor : ExpressionVisitor
    {
        public string GetSqlQuery(Expression node) {
            this.Visit(node);

            throw new NotImplementedException();
        }

        
    }
}
