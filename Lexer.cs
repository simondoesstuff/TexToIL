using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace LatexProcessing2
{
    public class Lexer
    {
        public static IEnumerable<MathToken> Lex(string expression)
        {
            // spaces in the expression should be ignored
            expression = expression.TrimStart();

            // there was no meaningful expression passed in
            if (expression.Length == 0)
            {
                yield break;
            }
            
            // main lexing process

            Match m;    // reused in each branch
            
            // ------------------------------------------------------------
            //              Variable
            // ------------------------------------------------------------
            
            m = Regex.Match(expression, @"^[a-zA-Z]");
            if (m.Success)
            {
                yield return MathToken.FromVariable(m.Value);
                foreach (var t in Lex(expression.Substring(m.Length))) yield return t;
                yield break;
            }
            
            // ------------------------------------------------------------
            //              Number
            // ------------------------------------------------------------
            
            m = Regex.Match(expression, @"^\d*\.?\d+");
            if (m.Success)
            {
                // here we are parsing a string into a number... that regex better be perfect
                yield return MathToken.FromNumber(int.Parse(m.Value));
                foreach (var t in Lex(expression.Substring(m.Length))) yield return t;
                yield break;
            }
            
            // ------------------------------------------------------------
            //              Add, Sub, Multiply, Frac, Root, Power
            // ------------------------------------------------------------
            
            m = Regex.Match(expression, @"^([\+\-]|\\cdot|\\frac|\\sqrt|\^)");
            if (m.Success)
            {
                switch (m.Value)
                {
                    case "+":
                        yield return MathToken.FromElement(MathElement.Add);
                        break;
                    case "-":
                        yield return MathToken.FromElement(MathElement.Subtract);
                        break;
                    case "\\cdot":
                        yield return MathToken.FromElement(MathElement.Multiply);
                        break;
                    case "\\frac":
                        yield return MathToken.FromElement(MathElement.Frac);
                        break;
                    case "\\sqrt":
                        yield return MathToken.FromElement(MathElement.Root);
                        break;
                    case "^":
                        yield return MathToken.FromElement(MathElement.Power);
                        break;
                }
                
                foreach (var t in Lex(expression.Substring(m.Length))) yield return t;
                yield break;
            }

            // ------------------------------------------------------------
            //              Parenthesis
            // ------------------------------------------------------------
            
            m = Regex.Match(expression, @"^(?:\\(left)\(|\\(right)\))");
            if (m.Success)
            {
                var type = m.Groups[1].Success ? MathElement.OpenParenthesis : MathElement.CloseParenthesis;
                yield return MathToken.FromElement(type);
                foreach (var t in Lex(expression.Substring(m.Length))) yield return t;
                yield break;
            }
            
            // ------------------------------------------------------------
            //              Latex Delimiter       {  }
            // ------------------------------------------------------------
            
            m = Regex.Match(expression, @"^(\{|\})");
            if (m.Success)
            {
                var type = m.Value.Equals("{") ? MathElement.OpenLatexDelimiter : MathElement.CloseLatexDelimiter;
                yield return MathToken.FromElement(type);
                foreach (var t in Lex(expression.Substring(m.Length))) yield return t;
                yield break;
            }
            
            // ------------------------------------------------------------
            //              At this point, the lexer should have finished
            // ------------------------------------------------------------

            if (!m.Success)
            {
                throw new ArgumentException("Unrecognizable expression: " + expression);
            }
        }
    }
}