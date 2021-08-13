namespace LatexProcessing
{
    public readonly struct MathOpToken : IMathToken
    {
        public MathTokenTypes TokenType => MathTokenTypes.Op;
        public MathOperatorTokenTypes OpType { get; }

        public MathOpToken(MathOperatorTokenTypes opType)
        {
            OpType = opType;
        }
        
        public override string ToString()
        {
            return $"(Op, {OpType})";
        }
    }
}