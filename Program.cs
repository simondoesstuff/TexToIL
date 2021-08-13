using System;
using System.Linq.Expressions;

namespace LatexProcessing
{
    public class Program
    {
        public void Main()
        {
            // var testExp = @"-\left(5+3\right)-40\pi n+3x\left(2--3\right)^{-4\left(1+5x\right)}-5\frac{1}{x}";
            // var testExp = @"-\left(5+3\right)-40\pi n+3x\left(2-3\right)^{-4\left(1+5x\right)}-5\frac{1}{x}";
            // var testExp = @"10\pi n+5-3\cdot9";
            // var testExp = @"-n \pi +5-3\cdot 9";
            // var testExp = @"x=\frac{-b+ \sqrt{b^2-4ac}}{2a}";
            // var testExp = @"x=\frac{-b+ b^2-4ac}{2a}";
            // var testExp = @"-3x^2+a^{-5-ab}+5^5";
            // var testExp = @"-3\cdot x^2+a^{-5-a\cdot b}+5^5";
            // var testExp = @"5\cdot \left(1-3\right)";

            var testExp = @"a\cdot\left(b+1\right)";
            
            var infixTokens = new LatexMathTokenizer().Lex(testExp);
            var postfixTokens = new InfixToPostfix().Convert(infixTokens);
            (var exp, var expParams) = new VirtualExpressionBuilder().Build(postfixTokens);

            Console.Out.WriteLine("");
            
            foreach (var parameterExpression in expParams)
            {
                Console.Out.WriteLine("Detected Parameter (1): " + parameterExpression);
            }
            
            var cParam = Expression.Parameter(typeof(double), "c");
            var compositeExp = Expression.Add(cParam, exp);

            var expParamsEnumerator = expParams.GetEnumerator();
            expParamsEnumerator.MoveNext();
            var aParam = expParamsEnumerator.Current;
            expParamsEnumerator.MoveNext();
            var bParam = expParamsEnumerator.Current;
            
            var expLambda = Expression.Lambda(compositeExp, cParam, bParam, aParam);
            
            foreach (var parameterExpression in expLambda.Parameters)
            {
                Console.Out.WriteLine("Detected Parameter (2): " + parameterExpression);
            }

            var compiledExp = expLambda.Compile();
            // var result = compiledExp.DynamicInvoke(4, 4, 5);
            var result = compiledExp.DynamicInvoke(5, 4, 4);
            Console.Out.WriteLine("result = {0}", result);
        }

        public void Test()
        {
            var const5 = Expression.Constant(5D);
            var paramA = Expression.Parameter(typeof(double), "a");
            var assignExp = Expression.Assign(paramA, Expression.Constant(10D));
            var exp = Expression.Add(paramA, const5);

            var blockExp = Expression.Block(new[] {Expression.Parameter(typeof(string))}, exp);

            var lambdaExp = Expression.Lambda(blockExp, paramA);

            var result = lambdaExp.Compile().DynamicInvoke(50D);
            Console.Out.WriteLine("result = {0}", result);
        }

        public static void Main(string[] args)
        {
            new Program().Test();
            // new Program().Main();
        }
    }
}