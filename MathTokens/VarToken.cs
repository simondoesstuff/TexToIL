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
        
        public override bool Equals(object? obj)
        {
            if (base.Equals(obj)) return true;
            
            var other = obj as VarToken;
        
            if (other == null) return false;
        
            if (this.Name != other.Name) return false;
            if (this.TokenType != other.TokenType) return false;
            if (this.ExpressionPosition != other.ExpressionPosition) return false;
        
            return true;
        }
        
        public override string ToString()
        {
            return $"(Var, '{Name}')";
        }
    }
}