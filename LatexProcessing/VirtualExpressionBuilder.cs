#define DEBUGPRINT


using System.Collections.Generic;
using System.Linq.Expressions;

namespace LatexProcessing
{
    public class VirtualExpressionBuilder
    {
        private Stack<Expression> _stack = new Stack<Expression>();
        private List<ParameterExpression> _parameterExpressions = new List<ParameterExpression>();

        public (Expression, IEnumerable<ParameterExpression>) Build(LexedLatexExp tokens)
        {
            foreach (var token in tokens)
            {
                _HandleToken(token);
            }

            return (_stack.Pop(), _parameterExpressions);
        }

        [DevNote("sqrt not supported yet")]
        private void _HandleToken(IMathToken token)
        {
            // handle operands

            if (token.TokenType == MathTokenTypes.Const)
            {
                var exp = Expression.Constant(((MathConstToken)token).Value);
                _stack.Push(exp);
                return;
            }
            
            if (token.TokenType == MathTokenTypes.Var)
            {
                var varToken = ((MathVarToken)token);
                var exp = Expression.Parameter(typeof(double), varToken.Name.ToString());
                _parameterExpressions.Add(exp);
                _stack.Push(exp);
                return;
            }

            // handle operators

            MathOpToken opToken;

            if (token.TokenType == MathTokenTypes.Op) opToken = (MathOpToken)token;
            else return;

            switch (opToken.OpType)
            {
                case MathOperatorTokenTypes.Add:
                    _HandleBinaryOps(Expression.Add);
                    break;
                case MathOperatorTokenTypes.Sub:
                    _HandleBinaryOps(Expression.Subtract);
                    break;
                case MathOperatorTokenTypes.Mult:
                    _HandleBinaryOps(Expression.Multiply);
                    break;
                case MathOperatorTokenTypes.Divide:
                    _HandleBinaryOps(Expression.Divide);
                    break;
                case MathOperatorTokenTypes.Expo:
                    _HandleBinaryOps(Expression.Power);
                    break;
            }
        }

        private delegate BinaryExpression BinaryExp(Expression a, Expression b);

        private void _HandleBinaryOps(BinaryExp func)
        {
            if (_stack.Count < 2) throw new InvalidLatexExpressionException("Expression not in valid postfix form.");
            
            var a = _stack.Pop();
            var b = _stack.Pop();
            var exp = func(b, a);
            _stack.Push(exp);
        }
    }
}