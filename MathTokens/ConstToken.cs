namespace LatexProcessing
{
    public readonly struct ConstToken : IMathToken
    {
        public MathTokenTypes TokenType => MathTokenTypes.Const;
        public int? ExpressionPosition { get; }

        public double Value { get; }

        public ConstToken(double value, int? expressionPosition)
        {
            Value = value;
            ExpressionPosition = expressionPosition;
        }

        public override string ToString()
        {
            return $"(Const, {Value})";
        }
    }
}