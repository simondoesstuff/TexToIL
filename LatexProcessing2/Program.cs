using System;
using System.Diagnostics;
using System.Linq;
using LatexProcessing2.Parsing;

/**
 * I started on this project about 1 year ago. This fully-fledged alegebra
 * parser was my first large parser at the time. I really struggled to get it
 * operational over more than week of effort and it was not even capable of
 * every feature I had in mind. I ultimately found a more clever way
 * to avoid needing the parser (for another project) and dropped the project.
 *
 * Yesterday, I deicided I would try to recreate the project to test myself.
 * I wrote over 700 lines in one day and got the entire project fully
 * operational. The next day I wrote 200 more and published it.
 *
 * It uses a highly recursive algorithm. Most similar to recursive-descent.
 * Binary operators are parsed first and the "terms" (operands) are parsed
 * seperately. Terms are broken into pieces (2abc = 2 * a * b * c) and
 * recombined into a single Expression unit.
 *
 * It parses perfectly efficiently, quickly, and can do anything I would need.
 */

namespace LatexProcessing2
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            while (true)
            {
                try
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
                }
                catch (Exception e)
                {
                    Console.Out.WriteLine(e);
                }
                finally
                {
                    Console.Out.WriteLine("");
                }
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
            catch (Exception)
            {
                Console.Out.WriteLine("Parameters must be in form:    a, b, c");
                foundParams = null;
                return false;
            }

            return true;
        }
    }
}