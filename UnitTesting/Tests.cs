using System;
using System.Linq;
using LatexProcessing2;
using NUnit.Framework;

namespace UnitTesting
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void TestLexer()
        {
            const string expression = @"\frac{-b + \sqrt{b^2-4ac}}{2a}";

            var expected =
                "Frac OpenLatexDelimiter Subtract Variable(b) Add Root OpenLatexDelimiter Variable(b) Power Number(2) Subtract Number(4) Variable(a) Variable(c) ClosedLatexDelimiter ClosedLatexDelimiter OpenLatexDelimiter Number(2) Variable(a) ClosedLatexDelimiter ";

            string actual = "";

            foreach (var token in Lexer.Lex(expression))
            {
                actual += token.Type.ToString();

                if (token.Type == MathElement.Number)
                {
                    actual += $"({token.NumberValue})";
                }
                else if (token.Type == MathElement.Variable)
                {
                    actual += $"({token.VariableName})";
                }
                
                actual += " ";
            }

            Console.Out.WriteLine(actual);
            StringAssert.AreEqualIgnoringCase(expected, actual);
        }
        
        [Test]
        public void TestFullParser()
        {
            const string expression = @"\frac{-b + \sqrt{b^2-4ac}}{2a}";
            Parser.Parse(Lexer.Lex(expression).ToList());
        }
    }
}