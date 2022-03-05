using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace LatexProcessing2.Parsing
{
    public class LatexFunction
    {
        public string[] Parameters {private set; get;}
        public Expression InternalExpression {private set; get;}
        public MathFunction Call { private set; get; }

        public delegate double MathFunction(params double[] args);
    
        private LatexFunction()
        {
        }
    
        public static LatexFunction CompileFrom(string expression)
        {
            var inst = new LatexFunction();
            var tokens = Lexer.Lex(expression).ToList();
            (var exp, var paramRefs) = Parser.BuildExpression(tokens);

            // initialize the Parameters field
            var paramKeysList = paramRefs.Keys.ToList();
            paramKeysList.Sort();
            inst.Parameters = paramKeysList.ToArray();

            // sort the ParameterExpressions by the Parameters field
            var paramValuesList = new List<ParameterExpression>();
            foreach (var param in inst.Parameters) 
                paramValuesList.Add(paramRefs[param]);

            // compile the expression
            var lambdaExp = Expression.Lambda(exp, paramValuesList);
            var funcDelegate = lambdaExp.Compile();
            inst.Call = args => (double)funcDelegate.DynamicInvoke(args.Select(d => (object) d).ToArray())!;
            
            return inst;
        }
    }
}