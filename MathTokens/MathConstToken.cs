namespace LatexProcessing
{
    public readonly struct MathConstToken : IMathToken
    {
        public MathTokenTypes TokenType => MathTokenTypes.Const;
        public double Value { get; }

        public MathConstToken(double value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return $"(Const, {Value})";
        }
    }
}