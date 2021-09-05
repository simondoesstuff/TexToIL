using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace LatexProcessing.LatexMathParser
{
    public class Parser
    {
        private static MethodInfo _sqrtMethod = typeof(Math).GetMethod("Sqrt", new[] { typeof(double) })!;
        private static MethodInfo _sinMethod = typeof(Math).GetMethod("Sin", new[] { typeof(double) })!;
        private static MethodInfo _cosMethod = typeof(Math).GetMethod("Cos", new[] { typeof(double) })!;
        
        // todo: unary negative not working
        public static (LambdaExpression, char?) BuildExpression(string latexExpression)
        {
            var stream = Lexer.Inst.Lex(latexExpression);
            stream = _OperationsFilter(stream);

            var iter = stream.GetEnumerator();
            char? variableAssignId = null;
            
            if (iter.MoveNext())
            {
                var current = iter.Current;

                if (current.TokenType == MathTokenTypes.Assignment)
                    variableAssignId = ((AssignmentToken)current).Name;
                else
                {
                    // reset the iterator
                    iter.Dispose();
                    iter = stream.GetEnumerator();
                }
            }
            
            // begin parsing

            var parameters = new Dictionary<char, ParameterExpression>();
            var parsedExpression = _Parse(iter, ref parameters);

            var sortedDict = from entry in parameters orderby entry.Key select entry.Value;

            var lambda = Expression.Lambda(parsedExpression, sortedDict);
            return (lambda, variableAssignId);
        }

        /// <summary>
        /// Injects LaTeX delimiters around the term sin or cos is being applied to.
        /// Eg: \sin x^2   becomes   \sin{x^2}
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private static IEnumerable<IMathToken> _SinCosFilter(IEnumerable<IMathToken> stream)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Injects implied multiplication operators when necessary.
        /// Withholds extraneous +/- operators.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private static IEnumerable<IMathToken> _OperationsFilter(IEnumerable<IMathToken> stream)    // todo: implied mult not working yet
        {
            IMathToken? previousToken = null;
            int collectedExtraneousSumOps = -1; // used to filter things like ++-+---+---
            
            Func<IMathToken?, bool> isValueType = (e) => e != null && e.TokenType switch
            {
                MathTokenTypes.Const => true,
                MathTokenTypes.Var => true,
                MathTokenTypes.Sep => true,
                _ => false
            };
            
            Func<IMathToken?, bool> isAddOrSubOp = (e) => e != null && e.TokenType switch
            {
                MathTokenTypes.Op => ((OpToken) e).OpType switch
                {
                    MathOperatorTokenTypes.Add => true,
                    MathOperatorTokenTypes.Sub => true,
                    _ => false
                },
                _ => false
            };
            
            foreach (var nextToken in stream)
            {
                // handles double +/- ops
                if (isAddOrSubOp(nextToken))
                {
                    if (collectedExtraneousSumOps == -1)
                    {
                        // start collection

                        // we count sub, we ignore add
                        if (((OpToken)nextToken).OpType == MathOperatorTokenTypes.Sub)
                            collectedExtraneousSumOps = 1;
                        else collectedExtraneousSumOps = 0;

                        previousToken = nextToken;  // previousToken now represents the first op collected
                    }
                    else
                    {
                        // sub increases, add is ignored
                        if (((OpToken)nextToken).OpType == MathOperatorTokenTypes.Sub)
                            collectedExtraneousSumOps++;
                    }
                    
                    continue;   // absorb tokens until collecting process is over
                } else if (collectedExtraneousSumOps != -1)
                {
                    // return collection

                    yield return collectedExtraneousSumOps % 2 == 1
                        ? new OpToken(MathOperatorTokenTypes.Sub, previousToken!.ExpressionPosition)
                        : new OpToken(MathOperatorTokenTypes.Add, previousToken!.ExpressionPosition);

                    collectedExtraneousSumOps = -1;
                }
                
                // handle implied multiplication
                do      // this loop only runs once. It enables    break;    to exit the if statement
                {
                    if (isValueType(previousToken) && isValueType(nextToken))
                    {
                        if (previousToken == null)
                            break;

                        if (!(previousToken is SepToken prevSep) || !(nextToken is SepToken nextSep))
                            break;

                        if (MathSeparatorTokenTypes.LatexDelimiter == prevSep.SepType &&
                            prevSep.SepType == nextSep.SepType)
                            break;

                        if (!prevSep.IsClosingSep || nextSep.IsClosingSep)
                            break;

                        yield return new OpToken(MathOperatorTokenTypes.Mult, nextToken.ExpressionPosition);
                    }
                } while (false);

                previousToken = nextToken;
                yield return previousToken;
            }
        }

        private static IMathToken _ExpectToken(IEnumerator<IMathToken> stream, IMathToken expected)
        {
            IMathToken received = stream.Current;

            if (stream.MoveNext())
            {
                received = stream.Current;

                return _ExpectToken(received, expected);
            }

            throw new InvalidLatexExpressionException(received,
                "Hit end of expression early. Expected: " + expected);
        }
        
        private static IMathToken _ExpectToken(IMathToken received, IMathToken expected)
        {
            if (received.Equals(expected))
                return received;
            
            throw new InvalidLatexExpressionException(received, "Unexpected token. Expected: " + expected);
        }

        private static Expression _Parse(IEnumerator<IMathToken> stream, ref Dictionary<char, ParameterExpression> parameters)
        {
            var expressionStack = new Stack<(Expression, MathSeparatorTokenTypes?)>();  // the sep token type tuple element is because some
                                                                                        // composite operators in LaTeX require specific
                                                                                        // elements to come after them. Like, \frac{a}{b}.
                                                                                        // \frac a {b} should not compile.
            var operatorStack = new Stack<OpToken>();       // will only be storing binary operators.
                                                            // any op that uses latex delimiters is handled
                                                            // separately.

            void ApplyOperator(OpToken op)
            {
                switch (op.OpType)
                {
                    case MathOperatorTokenTypes.Sin:
                    case MathOperatorTokenTypes.Cos:
                    case MathOperatorTokenTypes.Sqrt:
                        // -------------------------------------------------------------------------------------
                        // handling unary operators:      sin, cos, sqrt
                        // we will make the assumption that the transcendental operators always come before a LaTeX delimiter sep token expression

                    {
                        if (expressionStack.Count < 1)
                            throw new InvalidLatexExpressionException(op, "Missing operand.");

                        var operandTuple = expressionStack.Pop();

                        if (!(operandTuple.Item2 is MathSeparatorTokenTypes.LatexDelimiter))
                            throw new InvalidLatexExpressionException(op, "Missing operand.");

                        var operand = operandTuple.Item1;

                        var combo = op.OpType switch
                        {
                            MathOperatorTokenTypes.Sin => Expression.Call(_sinMethod, operand),
                            MathOperatorTokenTypes.Cos => Expression.Call(_cosMethod, operand),
                            MathOperatorTokenTypes.Sqrt => Expression.Call(_sqrtMethod, operand)
                        };
                        
                        expressionStack.Push((combo, null));
                    }
                        
                        break;

                    case MathOperatorTokenTypes.Expo:
                        // -------------------------------------------------------------------------------------
                        // handling      expo
                        //
                        //      expo is easy because it can be in the form     a^b   or  {a}^{b}
                        //      so little validation is required.
                    {
                        var missingOperands = 2 - expressionStack.Count;
                        missingOperands = missingOperands < 0 ? 0 : missingOperands;

                        if (missingOperands > 0)
                        {
                            throw new InvalidLatexExpressionException(op, $"Missing {missingOperands} operand(s).");
                        }

                        var operand2 = expressionStack.Pop().Item1;
                        var operand1 = expressionStack.Pop().Item1;

                        var combo = Expression.Power(operand1, operand2);
                        expressionStack.Push((combo, null));
                    }
                        
                        break;

                    case MathOperatorTokenTypes.Frac:
                        // -------------------------------------------------------------------------------------
                        // handling      frac
                    {
                        var missingOperands = 2 - expressionStack.Count;
                        missingOperands = missingOperands < 0 ? 0 : missingOperands;

                        if (missingOperands > 0)
                        {
                            throw new InvalidLatexExpressionException(op, $"Missing {missingOperands} operand(s).");
                        }
                        
                        var operandTuple2 = expressionStack.Pop();
                        var operandTuple1 = expressionStack.Pop();

                        if (operandTuple2.Item2 != MathSeparatorTokenTypes.LatexDelimiter)
                            missingOperands++;
                        
                        if (operandTuple1.Item2 != MathSeparatorTokenTypes.LatexDelimiter)
                            missingOperands++;
                        
                        if (missingOperands > 0)
                        {
                            throw new InvalidLatexExpressionException(op, $"Missing {missingOperands} operand(s).");
                        }
                        
                        // here we can assume the operands are normal

                        var operand2 = operandTuple2.Item1;
                        var operand1 = operandTuple1.Item1;

                        var combo = Expression.Divide(operand1, operand2);
                        expressionStack.Push((combo, null));
                    }
                        
                        break;
                    
                    default:
                        // -------------------------------------------------------------------------------------
                        // handling binary operators:      add, sub, mult
                    {
                        var missingOperands = 2 - expressionStack.Count;
                        missingOperands = missingOperands < 0 ? 0 : missingOperands;

                        if (missingOperands > 0)
                        {
                            throw new InvalidLatexExpressionException(op, $"Missing {missingOperands} operand(s).");
                        }

                        var operand2 = expressionStack.Pop().Item1;
                        var operand1 = expressionStack.Pop().Item1;

                        Expression combo = (op.OpType) switch
                        {
                            MathOperatorTokenTypes.Add => Expression.Add(operand1, operand2),
                            MathOperatorTokenTypes.Sub => Expression.Subtract(operand1, operand2),
                            MathOperatorTokenTypes.Mult => Expression.Multiply(operand1, operand2),
                        };

                        expressionStack.Push((combo, null));
                    }
                        
                        break;
                }
            }
            
            // -------------------------------------------------------------------------------------

            // Assignment tokens will be ignored. It is impossible for one to show up here
            
            while (stream.MoveNext())
            {
                var nextToken = stream.Current;

                // -------------------------------------------------------------------------------------
                // constant values are immediately pushed
                if (nextToken.TokenType == MathTokenTypes.Const)
                {
                    var value = ((ConstToken) nextToken).Number;
                    expressionStack.Push((Expression.Constant(value), null));
                    continue;
                }
                
                // -------------------------------------------------------------------------------------
                // variables are converted to parameter expressions and immediately pushed
                if (nextToken.TokenType == MathTokenTypes.Var)
                {
                    var id = ((VarToken)nextToken).Name;

                    // variables of the same id use the same parameter expression instance
                    if (!parameters.ContainsKey(id))
                        parameters[id] = Expression.Parameter(typeof(double), id.ToString());
                    
                    var param = parameters[id];
                    expressionStack.Push((param, null));
                    continue;
                }

                // -------------------------------------------------------------------------------------
                // parenthesis and absval bars are recursively parsed and pushed
                if (nextToken.TokenType == MathTokenTypes.Sep)
                {
                    var t = ((SepToken) nextToken).SepType;
                    expressionStack.Push((_CaptureSepGroup(nextToken, stream, ref parameters), t));
                    continue;
                }

                // -------------------------------------------------------------------------------------
                // operator handling
                //      operators of higher precedence than stack top are
                //      pushed to the stack top. Operators of lower precedence
                //      wait for the stack top operator to apply to expression
                //      stack, then try again...
                if (nextToken.TokenType == MathTokenTypes.Op)
                {
                    OpToken opNextToken = (OpToken)nextToken;

                    int StackTopPrecedence()
                    {
                        if (!operatorStack.TryPeek(out var stackTop))
                            return -1;      // all normal operator precedence is at least 0
                        return _Precedence(stackTop.OpType);
                    }

                    while (_Precedence(opNextToken.OpType) <= StackTopPrecedence())
                    {
                        var popStack = operatorStack.Pop();

                        ApplyOperator(popStack);
                    }

                    operatorStack.Push(opNextToken);
                    continue;
                }
            }

            // any remaining operators will be applied
            // starting with the top of the stack
            foreach (var op in operatorStack)
            {
                ApplyOperator(op);
            }
            
            // in an ideal expression, the expressionStack
            // should now only contain 1 element.
            
            if (expressionStack.Count == 1)
                return expressionStack.Pop().Item1;

            throw new InvalidLatexExpressionException("Extraneous value. Are you missing an operator?");
        }

        /// <summary>
        /// Scans for group latex structures, compiles and returns them
        /// </summary>
        /// <param name="nextToken">MUST BE A SEPARATOR TOKEN</param>
        /// <param name="stream"></param>
        /// <param name="parameters"></param>
        /// <returns>Compiled expression representing the inside of the group (including abs bars)</returns>
        /// <exception cref="InvalidLatexExpressionException"></exception>
        private static Expression _CaptureSepGroup(IMathToken nextToken, IEnumerator<IMathToken> stream, ref Dictionary<char, ParameterExpression> parameters)
        {
            int lookingForClosing = 1;
            MathSeparatorTokenTypes sepTypeFilter = ((SepToken)nextToken).SepType;
            var insideTokens = new List<IMathToken>();

            while (stream.MoveNext())
            {
                var futureToken = stream.Current;

                // opening seps increase lookingForClosing and closing seps decrease
                if (futureToken.TokenType == MathTokenTypes.Sep && ((SepToken)futureToken).SepType == sepTypeFilter)
                {
                    if (((SepToken)futureToken).IsClosingSep)
                        lookingForClosing--;
                    else
                        lookingForClosing++;
                }

                // exit if we found the corresponding separator token
                if (lookingForClosing == 0) break;

                insideTokens.Add(futureToken);
            }

            if (lookingForClosing != 0)
            {
                string typeStr;

                if (sepTypeFilter == MathSeparatorTokenTypes.Parenthesis)
                    typeStr = "parenthesis";
                else if (sepTypeFilter == MathSeparatorTokenTypes.AbsoluteValue)
                    typeStr = "bar";
                else throw new InvalidLatexExpressionException(nextToken, "No matching LaTeX delimiter found. The expression must be in LaTeX form.");

                throw new InvalidLatexExpressionException(nextToken, $"No matching {typeStr} found.");
            }

            // at this point, we have successfully iterated from the...
            // ( -> to the -> ) and captured the internal contents.
            // Now, we will recursively parse the inside stuff and push
            // to the exp stack.

            var insideTokensIter = insideTokens.GetEnumerator();
            Expression insideExpression = _Parse(insideTokensIter, ref parameters);
            insideTokensIter.Dispose();

            // if we were dealing with abs val, we will apply that here
            if (sepTypeFilter == MathSeparatorTokenTypes.AbsoluteValue)
            {
                /*
                    Equivalent to:
                    
                    =>      exp < 0 ? 0 - exp : exp
                */

                insideExpression = Expression.Condition(
                    Expression.LessThan(insideExpression, Expression.Constant(0)),
                    Expression.Subtract(Expression.Constant(0), insideExpression),
                    insideExpression
                );
            }

            return insideExpression;
        }

        private static int _Precedence(MathOperatorTokenTypes type)
        {
            if (type == MathOperatorTokenTypes.Frac)
                return MathOperatorTokenTypes.Sqrt.GetPrecedence();

            return type.GetPrecedence();
        }
    }
}