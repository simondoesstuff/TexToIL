#define DEBUGPRINT


using System;
using System.Collections.Generic;

namespace LatexProcessing
{
    /// <summary>
    /// implementation to convert infix expression to postfix.
    /// </summary>
    public class InfixToPostfix
    {
        // initializing empty list for result
        private List<IMathToken> _result = new List<IMathToken>();

        // initializing empty stack
        private Stack<IMathToken> _stack = new Stack<IMathToken>();

        // The main method that converts given infix expression
        // to postfix expression. 
        public LexedLatexExp Convert(LexedLatexExp exp)
        {
            foreach (var c in exp)
            {
                _HandleToken(c);
            }

            // pop all the operators from the stack
            while (_stack.Count > 0)
            {
                _result.Add(_stack.Pop());
            }

#if DEBUGPRINT
            Console.Out.WriteLine("\nPostfix Output:");
            _result.ForEach(token => Console.Out.WriteLine(token));
#endif

            return new LexedLatexExp(_result, exp.VariableAssignment);
        }

        // A utility function to return
        // precedence of a given operator
        // Higher returned value means higher precedence
        private static int _Prec(IMathToken c)
        {
            if (c.TokenType != MathTokenTypes.Op) return -1;

            var cOp = (OpToken)c;

            return cOp.OpType.GetPrecedence();
        }

        private static bool _IsOpenOrClosePara(IMathToken c, bool isOpen = true)
        {
            if (c.TokenType != MathTokenTypes.Sep) return false;
            return !((SepToken)c).IsClosingSep == isOpen;
        }

        private void _HandleToken(IMathToken c)
        {
            // If the scanned character is an
            // operand, add it to output.
            if (c.TokenType == MathTokenTypes.Const || c.TokenType == MathTokenTypes.Var)
            {
                _result.Add(c);
            }

            // If the scanned character is an '(',
            // push it to the stack.
            else if (_IsOpenOrClosePara(c))
            {
                _stack.Push(c);
            }

            // If the scanned character is an ')',
            // pop and output from the stack
            // until an '(' is encountered.
            else if (_IsOpenOrClosePara(c, false))
            {
                while (_stack.Count > 0 &&
                       !_IsOpenOrClosePara(_stack.Peek()))
                {
                    _result.Add(_stack.Pop());
                }

                if (_stack.Count > 0 && !_IsOpenOrClosePara(_stack.Peek()))
                {
                    throw new InvalidLatexExpressionException(); // invalid expression
                }

                _stack.Pop();
            }
            else // an operator is encountered
            {
                while (_stack.Count > 0 && _Prec(c) <=
                    _Prec(_stack.Peek()))
                {
                    _result.Add(_stack.Pop());
                }

                _stack.Push(c);
            }
        }
    }

// This code is contributed by Shrikant13
}