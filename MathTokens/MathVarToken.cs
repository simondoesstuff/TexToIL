namespace LatexProcessing
{
    public readonly struct MathVarToken : IMathToken
    {
        public MathTokenTypes TokenType => MathTokenTypes.Var;
        public char Name { get; }

        public MathVarToken(char name)
        {
            Name = name;
        }
        
        public override string ToString()
        {
            return $"(Var, '{Name}')";
        }
    }
}