using System;
using System.Linq.Expressions;

namespace LatexProcessing2;

public static class ExtraLinqExpressions
{
    public static Expression Sqrt(Expression e)
    {
        var sqrtMethod = typeof(Math).GetMethod("Sqrt", new[] { typeof(double) });
        return Expression.Call(sqrtMethod, e);
    }
}