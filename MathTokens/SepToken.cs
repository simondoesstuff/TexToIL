namespace LatexProcessing
{
    public readonly struct SepToken : IMathToken
    {
        public MathTokenTypes TokenType => MathTokenTypes.Sep;
        public int? ExpressionPosition { get; }
        public MathSeparatorTokenTypes Type { get; }
        public bool IsClosingSep { get; }

        public SepToken(MathSeparatorTokenTypes type, bool isClosingSep, int? expressionPosition)
        {
            Type = type;
            IsClosingSep = isClosingSep;
            ExpressionPosition = expressionPosition;
        }
        
        public override string ToString()
        {
            return $"(Sep, {Type}, {(IsClosingSep ? "right" : "left")})";
        }
    }
}