using System;

namespace LatexProcessing
{
    public class AssignmentToken : VarToken
    {
        // public MathTokenTypes TokenType => MathTokenTypes.Var;
        
        public AssignmentToken(char name, int? expressionPosition) : base(name, expressionPosition)
        {
            TokenType = MathTokenTypes.Assignment;
        }

        public override string ToString()
        {
            return $"(Assign Var, '{Name}')";
        }
    }
}