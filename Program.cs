using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using LatexProcessing.LatexMathParser;

namespace LatexProcessing
{
    public class Program
    {
        double Time(Func<dynamic> func)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            func();
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }
        
        
        
        /*public void FullTest()
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
        }*/

        public void ExpTreeTest()
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

        public void LexerTest()
        {
            var lexer = Lexer.Inst;
            
            foreach (var token in lexer.Lex(@"P=a\cdot\left(b+1\right)+\sqrt{73\pi}"))
                Console.Out.WriteLine("token: " + token);
        }

        public void ParserTest()
        {
            var exp = @"3+10\cdot b";
        
            try
            {
                Console.Out.WriteLine("exp = {0}", exp);

                // measure the time to parse & compile
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var parseResult = Parser.BuildExpression(exp);

                // print variable assignment
                if (parseResult.Item2 != null)
                    Console.Out.WriteLine($"Var Assign: '{parseResult.Item2}'");

                // store lambda exp
                var lambdaExp = parseResult.Item1;
                
                // print parameters
                var parametersStr = string.Join(", ", lambdaExp.Parameters);
                Console.Out.WriteLine("Parameters: " + parametersStr);
                
                // compile expression, print compilation time
                var compiledExp = lambdaExp.Compile();
                stopwatch.Stop();
                Console.Out.WriteLine("Parsed and compiled in {0}ms", stopwatch.ElapsedMilliseconds);
                
                // get parameters from console in
                var userParams = from str in Console.ReadLine()?.Split(", ") select (object) double.Parse(str);

                // calculate the answer and print the time elapsed
                stopwatch.Restart();
                stopwatch.Start();
                double answer = (double) compiledExp.DynamicInvoke(userParams.ToArray())!;
                stopwatch.Stop();
                Console.Out.WriteLine("Answer calculated in {0}ms", stopwatch.ElapsedMilliseconds);
                
                // print the answer
                Console.Out.WriteLine("answer = {0}", answer);
            }
            catch (InvalidLatexExpressionException e)
            {
                Console.WriteLine(e.ToString());
                int? num = e.IndexOfError;
                Console.Out.WriteLine("Here: " + (num != null ? exp[(int)num..] : "entirety"));
                Console.Out.WriteLine("");
                Console.Out.WriteLine(e.StackTrace);
            }
        }

        public void MathTokenTest()
        {
            void AreSame(IMathToken one, IMathToken two)
            {
                Console.Out.WriteLine($"{(one.Equals(two) ? "Yes" : "No")}");
            }

            AreSame(new OpToken(MathOperatorTokenTypes.Add, 3),
                new OpToken(MathOperatorTokenTypes.Add, 3));
            
            AreSame(new OpToken(MathOperatorTokenTypes.Add, 4),
                new OpToken(MathOperatorTokenTypes.Add, 3));
            
            AreSame(new OpToken(MathOperatorTokenTypes.Sub, 3),
                new OpToken(MathOperatorTokenTypes.Add, 3));
            
            AreSame(new SepToken(MathSeparatorTokenTypes.AbsoluteValue, true, 6),
                new SepToken(MathSeparatorTokenTypes.AbsoluteValue, false, 6));
            
            AreSame(new VarToken('a', null),
                new AssignmentToken('a', null));
            
            AreSame(new AssignmentToken('a', null),
                new VarToken('a', null));
        }
        
        public static void Main(string[] args)
        {
            // new Program().ExpTreeTest();
            // new Program().FullTest();
            // new Program().LexerTest();
            new Program().ParserTest();
            // new Program().MathTokenTest();
        }
    }
}