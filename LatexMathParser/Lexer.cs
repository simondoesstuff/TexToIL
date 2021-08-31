﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace LatexProcessing.LatexMathParser
{
    /// Singleton
    public class Lexer
    {
        private Regex _reAssignment = new Regex(@"^([a-z])=");
        private Regex _reDelimiter = new Regex(@"^({|})");
        private Regex _reAdd = new Regex(@"^\+");
        private Regex _reSub = new Regex(@"^-");
        private Regex _reMult = new Regex(@"^\\cdot");
        private Regex _reExpo = new Regex(@"^\^");
        private Regex _reConst = new Regex(@"^(\d+)");
        private Regex _reVar = new Regex(@"^([a-z])");
        private Regex _rePi = new Regex(@"^\\pi");
        private Regex _reFrac = new Regex(@"^\\frac");
        private Regex _reSqrt = new Regex(@"^\\sqrt");
        private Regex _reParenthesis = new Regex(@"^\\(left|right)(?:\(|\))");
        private Regex _reAbsVal = new Regex(@"^\\(left|right)\|");

        private static readonly Lazy<Lexer> _lazy = new Lazy<Lexer>(() => new Lexer());
        public static Lexer Inst => _lazy.Value;
        
        private Lexer() {}

        private void _PreFilter(ref string str)
        {
            str = str.ToLower();
        }

        public IEnumerable<IMathToken> Lex(string latexExp)
        {
            _PreFilter(ref latexExp);

            int index = 0;
            
            // do an initial match for variable assignment
            Match match = _reAssignment.Match(latexExp);
            if (match.Success)
            {
                yield return new AssignmentToken(match.Groups[1].Value[0], index);
                index += match.Length;
            }

            // iterate through expression lexing each token as we go
            do yield return _LexOne(latexExp[index..], ref index);
            while (index < latexExp.Length);
        }

        private IMathToken _LexOne(string latexExp, ref int stackPointer)
        {
            Match match;

            match = _reDelimiter.Match(latexExp);
            if (match.Success)
            {
                stackPointer += match.Length;
                var group1 = match.Groups[1].Value;

                if (group1[0] == '{')
                    return new SepToken(MathSeparatorTokenTypes.LatexDelimiter, false, stackPointer);

                return new SepToken(MathSeparatorTokenTypes.LatexDelimiter, true, stackPointer);
            }
            
            match = _reAdd.Match(latexExp);
            if (match.Success)
            {
                stackPointer += match.Length;
                return new OpToken(MathOperatorTokenTypes.Add, stackPointer);
            }
            
            match = _reSub.Match(latexExp);
            if (match.Success)
            {
                stackPointer += match.Length;
                return new OpToken(MathOperatorTokenTypes.Sub, stackPointer);
            }
            
            match = _reMult.Match(latexExp);
            if (match.Success)
            {
                stackPointer += match.Length;
                return new OpToken(MathOperatorTokenTypes.Mult, stackPointer);
            }
            
            match = _reExpo.Match(latexExp);
            if (match.Success)
            {
                stackPointer += match.Length;
                return new OpToken(MathOperatorTokenTypes.Expo, stackPointer);
            }
            
            match = _reConst.Match(latexExp);
            if (match.Success)
            {
                stackPointer += match.Length;
                
                if (Double.TryParse(match.Groups[1].Value, out var value))
                    return new ConstToken(value, stackPointer);
            }
            
            match = _reVar.Match(latexExp);
            if (match.Success)
            {
                stackPointer += match.Length;
                return new VarToken(match.Groups[1].Value[0], stackPointer);
            }
            
            match = _rePi.Match(latexExp);
            if (match.Success)
            {
                stackPointer += match.Length;
                return new ConstToken(Math.PI, stackPointer);
            }
            
            match = _reFrac.Match(latexExp);
            if (match.Success)
            {
                stackPointer += match.Length;
                return new OpToken(MathOperatorTokenTypes.Frac, stackPointer);
            }
            
            match = _reSqrt.Match(latexExp);
            if (match.Success)
            {
                stackPointer += match.Length;
                return new OpToken(MathOperatorTokenTypes.Sqrt, stackPointer);
            }
            
            match = _reParenthesis.Match(latexExp);
            if (match.Success)
            {
                stackPointer += match.Length;
                
                if (match.Groups[1].Value[0] == 'l')
                    return new SepToken(MathSeparatorTokenTypes.Parenthesis, false, stackPointer);
                return new SepToken(MathSeparatorTokenTypes.Parenthesis, true, stackPointer);
            }
            
            match = _reAbsVal.Match(latexExp);
            if (match.Success)
            {
                stackPointer += match.Length;
                
                if (match.Groups[1].Value[0] == 'l')
                    return new SepToken(MathSeparatorTokenTypes.AbsoluteValue, false, stackPointer);
                return new SepToken(MathSeparatorTokenTypes.AbsoluteValue, true, stackPointer);
            }

            throw new InvalidLatexExpressionException(stackPointer);
        }
    }
}