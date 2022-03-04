using System;
using System.Diagnostics;
using System.Linq;
using LatexProcessing2.Parsing;

namespace LatexProcessing2
{
    // todo this project is not yet synced to github

    internal class Program
    {
        public static void Main(string[] args)
        {
            while (true)
            {
                // get the expression from console in
                Console.Out.Write("Expression > ");
                string expression = Console.ReadLine();
                var latexFunc = Compile(expression);

                // print parameters
                var detectedParams = string.Join(", ", latexFunc.Parameters);
                Console.Out.WriteLine($"Detected {latexFunc.Parameters.Length} Parameter(s): {detectedParams}");

                // get parameters from console in
                double[] executionParams;
                while (true)
                    if (GetParams(out executionParams, latexFunc.Parameters.Length))
                        break;

                // print the answer
                var answer = latexFunc.Call(executionParams);
                Console.Out.WriteLine("answer = {0}", answer);
                Console.Out.WriteLine("");
            }
        }

        private static LatexFunction Compile(string expression)
        {
            // measure the time to parse & compile
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var latexFunc = LatexFunction.CompileFrom(expression);
            stopwatch.Stop();
            Console.Out.WriteLine("Parsed and compiled in {0}ms", stopwatch.ElapsedMilliseconds);

            return latexFunc;
        }

        private static bool GetParams(out double[] foundParams, int requiredLength)
        {
            if (requiredLength == 0)
            {
                foundParams = Array.Empty<double>();
                return true;
            }
            
            try
            {
                Console.Out.Write("Parameters > ");
                var userParams = Console.ReadLine()?.Split(", ");
                foundParams = (from str in userParams select double.Parse(str)).ToArray();

                if (foundParams.Length != requiredLength)
                {
                    Console.Out.WriteLine($"Expected {requiredLength} parameter(s).");
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.Out.WriteLine("Parameters must be in form:    a, b, c");
                foundParams = null;
                return false;
            }

            return true;
        }
    }
}