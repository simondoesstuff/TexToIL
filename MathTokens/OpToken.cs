namespace LatexProcessing
{
    public readonly struct OpToken : IMathToken
    {
        public MathTokenTypes TokenType => MathTokenTypes.Op;
        public int? ExpressionPosition { get; }
        public MathOperatorTokenTypes OpType { get; }

        public OpToken(MathOperatorTokenTypes opType, int? expressionPosition)
        {
            OpType = opType;
            ExpressionPosition = expressionPosition;
        }
        
        public override string ToString()
        {
            return $"(Op, {OpType})";
        }
    }
}