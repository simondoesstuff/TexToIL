namespace LatexProcessing
{
    public interface IMathToken
    {
        public MathTokenTypes TokenType { get; }
        public int? ExpressionPosition { get; }
    }
}