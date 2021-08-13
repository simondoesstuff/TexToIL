namespace LatexProcessing
{
    public readonly struct MathSepToken : IMathToken
    {
        public MathTokenTypes TokenType => MathTokenTypes.Sep;
        public MathSeparatorTokenTypes Type { get; }
        public bool IsClosingSep { get; }

        public MathSepToken(MathSeparatorTokenTypes type, bool isClosingSep)
        {
            Type = type;
            IsClosingSep = isClosingSep;
        }
        
        public override string ToString()
        {
            return $"(Sep, {Type}, {(IsClosingSep ? "right" : "left")})";
        }
    }
}