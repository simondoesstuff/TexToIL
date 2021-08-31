namespace LatexProcessing
{
    public class SepToken : IMathToken
    {
        public MathTokenTypes TokenType => MathTokenTypes.Sep;
        public int? ExpressionPosition { get; }
        public MathSeparatorTokenTypes SepType { get; }
        public bool IsClosingSep { get; }

        public SepToken(MathSeparatorTokenTypes sepType, bool isClosingSep, int? expressionPosition)
        {
            SepType = sepType;
            IsClosingSep = isClosingSep;
            ExpressionPosition = expressionPosition;
        }
        
        public override bool Equals(object? obj)
        {
            if (base.Equals(obj)) return true;
            
            var other = obj as SepToken;
        
            if (other == null) return false;
        
            if (this.SepType != other.SepType) return false;
            if (this.IsClosingSep != other.IsClosingSep) return false;
            if (this.ExpressionPosition != other.ExpressionPosition) return false;
        
            return true;
        }
        
        public override string ToString()
        {
            return $"(Sep, {SepType}, {(IsClosingSep ? "right" : "left")})";
        }
    }
}