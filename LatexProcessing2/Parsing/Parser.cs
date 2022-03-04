using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace LatexProcessing2.Parsing
{
    // todo custom exception handling
    
    public class Parser
    {
        public static (Expression, Dictionary<string, ParameterExpression>) BuildExpression(List<MathToken> expression)
        {
            var parser = new Parser();
            var exp = parser.Parse(expression);
            return (exp, parser._parameters);
        }

        private Parser()
        {
        }
        
        // --------------------------------------------------
        //      Parsing Algorithm
        // --------------------------------------------------
        
        private readonly Dictionary<string, ParameterExpression> _parameters = new();
        
        private Expression Parse(List<MathToken> expression)
        {
            // ------------------------------------------------------
            //          Handling binary operators inside
            //          Parse() and giving "value" tokens
            //          to TermParse() which will also handle
            //          unary operators.
            // ------------------------------------------------------

            if (expression.Count == 0)
            {
                throw new Exception("Expected expression. Misformatted Latex.");
            }
            
            Expression result;

            result = ParseAddOrSub(expression);
            if (result != null) return result;    // if successful
            
            result = ParseMult(expression);
            if (result != null) return result;    // if successful

            // at this point, there are no binary operators on the top depth level
            // so the entire expression is a single term --> TermParse()

            return TermParse(expression);
        }

        private Expression ParseAddOrSub(List<MathToken> expression)
        {
            int depth = 0;

            for (int i = 0; i < expression.Count; i++)
            {
                var token = expression[i];
                
                depth += token.isOpeningElement() ? 1 : 0;
                depth += token.isClosingElement() ? -1 : 0;

                // only parse over depth 0 tokens -- skip parenthesis blocks
                if (depth != 0) continue;

                switch (token.Type)
                {
                    case MathElement.Add:
                        return Expression.Add(
                            Parse(expression.Take(i).ToList()),
                            Parse(expression.Skip(i + 1).ToList())
                        );
                    case MathElement.Subtract:
                        // we first need to confirm this is subtraction
                        // and not unary negation
                        
                        /*
                         * This method is called before the Terms (TermParse) are parsed
                         * and it only operates at the highest depth level. So we can
                         * ignore group elements.
                         * 
                         * Unary Negation:
                         *  Prefixed by the edge of the expression within the same depth
                         *  or a multiplication operator.
                         *
                         * Subtraction:
                         *  In-between two terms.
                         */

                        // we are dealing with a unary sub operator
                        if (i == 0 || expression[i - 1].Type == MathElement.Multiply) continue;
                        
                        return Expression.Subtract(
                            TermParse(expression.Take(i).ToList()),
                            TermParse(expression.Skip(i + 1).ToList())
                        );
                }
            }

            // at this point, it did not find any binary add or sub operators
            return null;
        }
        
        private Expression ParseMult(List<MathToken> expression)
        {
            int depth = 0;

            for (int i = 0; i < expression.Count; i++)
            {
                var token = expression[i];
                
                depth += token.isOpeningElement() ? 1 : 0;
                depth += token.isClosingElement() ? -1 : 0;

                // only parse over depth 0 tokens -- skip parenthesis blocks
                if (depth != 0) continue;

                if (token.Type != MathElement.Multiply) continue;

                return Expression.Multiply(
                    Parse(expression.Take(i).ToList()),
                    Parse(expression.Skip(i + 1).ToList())
                );
            }

            // at this point, it did not find any binary add or sub operators
            return null;
        }

        private Expression TermParse(List<MathToken> expression)
        {
            /*
             * A term is a row a "value tokens". Eg:     3(5 \sqrt( 5 ) ).
             * There are no binary operators. Each individual term... (3, 5, \sqrt)
             * is handled separately and multiplied together.
             */
            
            // this could happen if -- eg:     " 1 + 2 * " (no term after the multiply)
            if (expression.Count == 0)
            {
                throw new Exception("Expected term. Misformatted Latex.");
            }

            var split = TermSplit(new List<Expression>(), expression);
            Expression combination = split[0];  // TermSplit will parse at least one term

            for (int i = 1; i < split.Count; i++)
            {
                var term = split[i];
                combination = Expression.Multiply(combination, term);
            }

            return combination;
        }
        
        // --------------------------------------------------
        //      Term Parse algorithmic methods
        // --------------------------------------------------
        
        /// <summary>
        /// Splits all the terms on the same depth level.
        /// They are combined into a single term in TermParse()
        /// </summary>
        /// <param name="split"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private List<Expression> TermSplit(List<Expression> split, List<MathToken> expression)
        {
            while (true)
            {
                // this could happen if -- eg:     " 1 + 2 * " (no term after the multiply)
                if (expression.Count == 0)
                {
                    return split;
                }

                var firstToken = expression[0];

                switch (firstToken.Type)
                {
                    // -------------------    Power    -------------------
                    case MathElement.Power:
                        PowerTermParse(ref split, ref expression);
                        continue;
                    // -------------------    Parenthesis    -------------------
                    case MathElement.OpenParenthesis:
                    {
                        var scan = DepthScan(expression, MathElement.OpenParenthesis, MathElement.CloseParenthesis);
                        var internalTerm = TermParse(expression.Skip(1).Take(scan - 1).ToList());
                        split.Add(internalTerm);
                        expression = expression.Skip(scan + 1).ToList();
                        continue;
                    }
                    // -------------------    Number    -------------------
                    case MathElement.Number:
                        split.Add(Expression.Constant(firstToken.NumberValue));
                        expression = expression.Skip(1).ToList();
                        continue;
                    // -------------------    Variable    -------------------
                    case MathElement.Variable:
                        split.Add(ParamFrom(firstToken.VariableName));
                        expression = expression.Skip(1).ToList();
                        continue;
                    // -------------------    Unary Negation    -------------------
                    case MathElement.Subtract:
                        // not sure if I should use NegateChecked or unchecked.
                        // Checked checks for overflow.
                        split.Add(Expression.NegateChecked(TermParse(expression.Skip(1).ToList())));
                        expression.Clear();     // unary negation can be thought of as an op that consumes the entire term
                        continue;
                    // -------------------    Root    -------------------
                    case MathElement.Root:
                        RootTermParse(ref split, ref expression);
                        continue;
                    // -------------------    Frac    -------------------
                    case MathElement.Frac:
                        FracTermParse(ref split, ref expression);
                        continue;
                }

                // todo sin, cos

                // at this point, this term is not parsable
                throw new Exception("Unexpected term. Misformatted Latex.");
            }
        }

        /// <summary>
        /// Given an openingType which increases the depth
        /// and a closingType which decreases the depth, scans
        /// from index 0 until the depth is 0. Utility method.
        ///
        /// Eg: (2+2)
        /// -->   4
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="openingType"></param>
        /// <param name="closingType"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static int DepthScan(List<MathToken> expression, MathElement openingType, MathElement closingType)
        {
            // We handle parenthesis by sending the entire block back through the main parser
            
            var firstToken = expression[0];
            
            if (firstToken.Type != openingType)
                throw new Exception("Misformatted Latex.");

            // we will now scan through the term to find the matching parenthesis
            for (int i = 1, depth = 1; i < expression.Count; i++)
            {
                var scanToken = expression[i];
                depth += scanToken.Type == openingType ? 1 : 0;
                depth += scanToken.Type == closingType ? -1 : 0;

                // only parse over depth 0 tokens -- skip parenthesis blocks
                if (depth != 0) continue;

                return i;
            }
            
            // at this point, this code branch did not find the valid parenthesis block
            throw new Exception("Misformatted Latex.");
        }

        private void PowerTermParse(ref List<Expression> split, ref List<MathToken> expression)
        {
            // assuming this will only be called when remaining[0] is a Power

            if (expression.Count < 2 || split.Count < 1)
                throw new Exception("Misformatted Latex.");

            // in A^B, referring to B
            Expression powerOperand;

            if (expression[1].Type != MathElement.OpenLatexDelimiter)
            {
                // we are dealing with a simple situation like    x^2
                powerOperand = TermParse(new List<MathToken> { expression[1] });
                expression = expression.Skip(2).ToList();
            }
            else
            {
                // we are dealing with a complex operand like    x ^ {3+2}
                var depthScan = DepthScan(expression.Skip(2).ToList(), MathElement.OpenLatexDelimiter, MathElement.ClosedLatexDelimiter);
                powerOperand = Parse(expression.Skip(2).Take(depthScan - 1).ToList());
                expression = expression.Skip(depthScan + 3).ToList();
            }
            
            // reconstruct the expression to include the power

            var baseOperand = split[split.Count - 1];
            var powerExp = Expression.Power(baseOperand, powerOperand);
            split[split.Count - 1] = powerExp;
        }

        private void RootTermParse(ref List<Expression> split, ref List<MathToken> expression)
        {
            var depthScan = DepthScan(expression.Skip(1).ToList(), MathElement.OpenLatexDelimiter, MathElement.ClosedLatexDelimiter);
            var internalOperand = Parse(expression.Skip(2).Take(depthScan - 1).ToList());
            var sqrtExp = ExtraLinqExpressions.Sqrt(internalOperand);
            var theRest = expression.Skip(depthScan + 2).ToList();
            
            split.Add(sqrtExp);
            expression = theRest;
        }

        private void FracTermParse(ref List<Expression> split, ref List<MathToken> expression)
        {
            // assuming expression[0] is the \frac

            if (expression.Count < 5)
                throw new Exception("Misformatted Latex");
            
            if (expression[1].Type != MathElement.OpenLatexDelimiter)
                throw new Exception("Misformatted Latex");
            
            var scan1 = DepthScan(expression.Skip(1).ToList(), MathElement.OpenLatexDelimiter, MathElement.ClosedLatexDelimiter);
            var numeratorTokens = expression.Skip(2).Take(scan1 - 1).ToList();
            var numerator = Parse(numeratorTokens);
            
            if (expression[scan1 + 2].Type != MathElement.OpenLatexDelimiter)
                throw new Exception("Misformatted Latex");
            
            var scan2 = DepthScan(expression.Skip(scan1 + 2).ToList(), MathElement.OpenLatexDelimiter, MathElement.ClosedLatexDelimiter);
            var denominatorTokens = expression.Skip(scan1 + 3).Take(scan2 - 1).ToList();
            var denominator = Parse(denominatorTokens);

            split.Add(Expression.Divide(numerator, denominator));
            expression = expression.Skip(scan1 + scan2 + 3).ToList();
        }
        
        // --------------------------------------------------
        //      Keep track of parameters
        // --------------------------------------------------

        /// <summary>
        /// Retrieve a parameter based on the name.
        /// If the parameter does not yet exist,
        /// it will be created.
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private ParameterExpression ParamFrom(string identifier)
        {
            if (!_parameters.ContainsKey(identifier))
            {
                _parameters.Add(identifier, Expression.Parameter(typeof(double), identifier));
            }

            return _parameters[identifier];
        }
    }
}