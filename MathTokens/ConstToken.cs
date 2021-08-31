namespace LatexProcessing
{
    public class ConstToken : IMathToken
    {
        public MathTokenTypes TokenType => MathTokenTypes.Const;
        public int? ExpressionPosition { get; }

        public double Number { get; }

        public ConstToken(double number, int? expressionPosition)
        {
            Number = number;
            ExpressionPosition = expressionPosition;
        }
        
        public override bool Equals(object? obj)
        {
            if (base.Equals(obj)) return true;
            
            var other = obj as ConstToken;
        
            if (other == null) return false;
        
            if (this.Number != other.Number) return false;
            if (this.ExpressionPosition != other.ExpressionPosition) return false;
        
            return true;
        }

        public override string ToString()
        {
            return $"(Const, {Number})";
        }
    }
}