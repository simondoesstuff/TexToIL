using System;

namespace LatexProcessing
{
    public class VarToken : IMathToken
    {
        public MathTokenTypes TokenType { get; protected set; }
        public int? ExpressionPosition { get; }
        public char Name { get; }

        /// <param name="name">Converted to lowercase</param>
        public VarToken(char name, int? expressionPosition)
        {
            TokenType = MathTokenTypes.Var;
            
            Name = Char.ToLower(name);
            ExpressionPosition = expressionPosition;
        }
        
        public override string ToString()
        {
            return $"(Var, '{Name}')";
        }
    }
}