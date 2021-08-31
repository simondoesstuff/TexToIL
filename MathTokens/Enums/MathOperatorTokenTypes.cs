using System;
using System.Diagnostics.CodeAnalysis;

namespace LatexProcessing
{
    public enum MathOperatorTokenTypes
    {
        Add,
        Sub,
        Mult,
        Frac,
        Expo,

        // unary operators
        Sin,
        Cos,
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
                MathOperatorTokenTypes.Frac => 1,
                MathOperatorTokenTypes.Expo => 2,
                
                // unary operators
                MathOperatorTokenTypes.Sin => 3,
                MathOperatorTokenTypes.Cos => 3,
                MathOperatorTokenTypes.Sqrt => 3
            };
        }

        public static bool IsUnary(this MathOperatorTokenTypes tokenEnum)
        {
            return tokenEnum switch
            {
                // unary operators
                MathOperatorTokenTypes.Sin => true,
                MathOperatorTokenTypes.Cos => true,
                MathOperatorTokenTypes.Sqrt => true,

                _ => false
            };
        }
    }
}