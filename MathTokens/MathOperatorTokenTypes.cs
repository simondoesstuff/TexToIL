using System.Diagnostics.CodeAnalysis;

namespace LatexProcessing
{
    public enum MathOperatorTokenTypes
    {
        Add,
        Sub,
        Mult,
        Divide,
        Expo,
        Sqrt
    }

    public static class MathOperatorTokenTypesMethods
    {
        public static int GetPrecedence([NotNull] this MathOperatorTokenTypes tokenEnum)
        {
            return tokenEnum switch
            {
                MathOperatorTokenTypes.Add => 0,
                MathOperatorTokenTypes.Sub => 0,
                MathOperatorTokenTypes.Mult => 1,
                MathOperatorTokenTypes.Divide => 1,
                MathOperatorTokenTypes.Expo => 2,
                MathOperatorTokenTypes.Sqrt => 3
            };
        }
    }
}