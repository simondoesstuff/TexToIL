using System;

namespace LatexProcessing2
{
    public class MathToken
    {
        public readonly MathElement Type;
        
        /// <summary>
        /// if not a MathElement.Number == null
        /// </summary>
        public readonly float? NumberValue;

        /// <summary>
        /// if not a MathElement.Variable == ""
        /// </summary>
        public readonly string VariableName;

        public static MathToken FromNumber(float number) => new MathToken(MathElement.Number, number, "");
        public static MathToken FromVariable(string name) => new MathToken(MathElement.Variable, null, name);

        /// <summary>
        /// Constructs a MathToken from any MathElement type as long
        /// as the type is not a Number or Variable. For that, use the
        /// other builder methods.
        /// </summary>
        /// <returns></returns>
        public static MathToken FromElement(MathElement type)
        {
            if (type == MathElement.Variable || type == MathElement.Number)
            {
                throw new ArgumentException("'type' must not be Variable or Number. Use the other builder methods.");
            }

            return new MathToken(type, null, "");
        }

        private MathToken(MathElement type, float? numberValue, string variableName)
        {
            this.Type = type;
            this.NumberValue = numberValue;
            this.VariableName = variableName;
        }
        
        public bool IsBinaryOperator()
        {
            switch (Type)
            {
                case MathElement.Add:
                case MathElement.Subtract:
                case MathElement.Multiply:
                case MathElement.Frac:
                    return true;
                default:
                    return false;
            }
        }
        
        public bool IsParenthesis()
        {
            switch (Type)
            {
                case MathElement.OpenParenthesis:
                case MathElement.CloseParenthesis:
                    return true;
                default:
                    return false;
            }
        }
        
        public bool IsLatexDelimiter()
        {
            switch (Type)
            {
                case MathElement.OpenLatexDelimiter:
                case MathElement.CloseLatexDelimiter:
                    return true;
                default:
                    return false;
            }
        }
    }
}