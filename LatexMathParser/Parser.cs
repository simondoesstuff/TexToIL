using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace LatexProcessing.LatexMathParser
{
    public class Parser
    {
        public static (Expression, char?) Parse(string latexExpression)
        {
            var stream = Lexer.Inst.Lex(latexExpression);
            stream = _Filter(stream);

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

            return (_BuildTree(iter), variableAssignId);
        }

        private static IEnumerable<IMathToken> _Filter(IEnumerable<IMathToken> stream)
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
                        ? new OpToken(MathOperatorTokenTypes.Sub, previousToken.ExpressionPosition)
                        : new OpToken(MathOperatorTokenTypes.Add, previousToken.ExpressionPosition);

                    collectedExtraneousSumOps = -1;
                }
                
                // handles implied multiplication
                if (isValueType(previousToken) && isValueType(nextToken))
                {
                    yield return new OpToken(MathOperatorTokenTypes.Mult, nextToken.ExpressionPosition);
                }
                
                previousToken = nextToken;
                yield return previousToken;
            }
        }

        private static Expression _BuildTree(IEnumerator<IMathToken> stream)
        {
            var expressionStack = new Stack<Expression>();
            var operatorStack = new Stack<OpToken>();

            while (stream.MoveNext())
            {
                var nextToken = stream.Current;

                Console.Out.WriteLine("nextToken = {0}", nextToken);
            }

            return null;
        }
    }
}