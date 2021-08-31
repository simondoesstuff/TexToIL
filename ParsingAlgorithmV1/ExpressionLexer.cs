#define DEBUGPRINT


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LatexProcessing
{
    /// <summary>
    ///     Only supports select latex tokens.
    /// </summary>
    [DevNote("Needs to filter double +/- ops")]
    public class LatexMathTokenizer
    {
        public static readonly string[] SupportedTokens =
        {
            "frac", "left\\(", "right\\)",
            "left\\|", "right\\|", "cdot",
            "sin", "cos", "pi", "sqrt"
        };

        private static readonly Regex ReFilterUnarySub = new Regex(@"(?<=^|\{|\(|\|)-");
        private static readonly Regex ReFilterBracketParenthesis = new Regex(@"\{(.*?)\}");

        [DevNote(@"Detecting \cdot\cdot")]
        private static readonly Regex ReFilterImpliedMult1 =
            new Regex(@"((?<!\\(?!cdot))\w)\s*(\\(?!cdot|right)(\w+))");

        private static readonly Regex ReFilterImpliedMult2 =
            new Regex(@"(?:(?<!\\[a-z]*)([a-z])|(?<!\d)(\d+)|(\\pi ?)){2,}");

        private static readonly Regex ReSubOp = new Regex(@"(?<!^|{|\\left. ?)-");
        private static readonly Regex ReAddOp = new Regex(@"\+");
        private static readonly Regex ReMultOp = new Regex(@"\\cdot ?");
        private static readonly Regex ReNumber = new Regex(@"(-?\d+)");
        private static readonly Regex RePiOrVar = new Regex(@"(-)?(?:(\\pi ?)|(?<!\\[a-z]*)([a-z]))");
        private static readonly Regex ReParenthesisAbsBars = new Regex(@"\\(?:(?:(left)|(right))(?:(\(|\))|(\|)))");
        private static readonly Regex ReExpoOp = new Regex(@"(\^)(?:(\{)\s*?(\}))?");
        private static readonly Regex ReFracOp = new Regex(@"(\\frac)(\{)\s*?(\})(\{)\s*?(\})");
        
        private List<IMathToken> _filteredTokens = new List<IMathToken>();
        private string _latexString;
        private readonly List<(int, IMathToken)> _tokens = new List<(int, IMathToken)>();

        public LexedLatexExp Lex(string latexString)
        {
            _latexString = latexString.ToLower();

            _VerifyAndFilter(); // preprocessing for the tokenizer

#if DEBUGPRINT
            Console.Out.WriteLine("\nfiltered exp = {0}", _latexString);
#endif

            _Tokenize();

#if DEBUGPRINT
            Console.Out.WriteLine("left over after tokenization = {0}", _latexString);
#endif

            _SortTokens();

#if DEBUGPRINT
            Console.Out.WriteLine("\nTokens:");
            foreach (var token in _tokens) Console.Out.WriteLine(token);
#endif

            return new LexedLatexExp(_filteredTokens);;
        }

        private void _Tokenize()
        {
            MatchCollection matches;

            // -------------------------------------------------------------------------------------------------
            matches = ReSubOp.Matches(_latexString);

            foreach (Match match in matches)
                _InsertToken(match.Index, new OpToken(MathOperatorTokenTypes.Sub));

            _WhiteOutString(matches);

            // -------------------------------------------------------------------------------------------------
            matches = ReAddOp.Matches(_latexString);

            foreach (Match match in matches)
                _InsertToken(match.Index, new OpToken(MathOperatorTokenTypes.Add));

            _WhiteOutString(matches);

            // -------------------------------------------------------------------------------------------------
            matches = ReMultOp.Matches(_latexString);

            foreach (Match match in matches)
                _InsertToken(match.Index, new OpToken(MathOperatorTokenTypes.Mult));

            _WhiteOutString(matches);

            // -------------------------------------------------------------------------------------------------
            matches = ReNumber.Matches(_latexString);

            foreach (Match match in matches)
            {
                // the match was pre-verified with regex. This should never fire.
                if (!int.TryParse(match.Value, out var result))
                    throw new InvalidLatexExpressionException(_latexString.Length - _latexString.Length);

                _InsertToken(match.Index, new ConstToken(result));
            }

            _WhiteOutString(matches);

            // -------------------------------------------------------------------------------------------------
            matches = RePiOrVar.Matches(_latexString);

            foreach (Match match in matches)
            {
                //      grous:
                //  only one of 2 & 3 can be successful.
                //
                //  0:ignored
                //  1:  contains a negative multiplier
                //  2:  contains pi
                //  3:  contains variable letter

                var groups = match.Groups;

                // ---   pi handling group
                // add pi.      On group 1 success, append -pi instead.
                if (groups[2].Success)
                {
                    _InsertToken(match.Index, new ConstToken(
                        (groups[1].Success ? -1 : 1) * Math.PI));

                    // if this group was successful, group 3 is definitely not successful.
                    continue;
                }

                // ---   variable handling group
                // group 1 handled by appending     -1 *
                if (groups[3].Success)
                {
                    if (groups[1].Success)
                    {
                        _InsertToken(match.Index, new ConstToken(-1));
                        _InsertToken(match.Index, new OpToken(MathOperatorTokenTypes.Mult));
                    }

                    var matchStr = groups[3].Value;

                    // due to regex verification, this should never fire
                    if (matchStr.Length > 1)
                        throw new InvalidLatexExpressionException(_latexString.Length - _latexString.Length,
                            matchStr.Length, "Variables must only be one character long.");

                    var matchVarNum = matchStr[0];

                    // again, impossible to fire due to regex verification
                    if (!char.IsLetter(matchVarNum))
                        throw new InvalidLatexExpressionException(_latexString.Length - _latexString.Length,
                            matchStr.Length, "Variables must be letters.");

                    _InsertToken(match.Index, new VarToken(matchVarNum));
                }
            }

            _WhiteOutString(matches);

            // -------------------------------------------------------------------------------------------------
            matches = ReParenthesisAbsBars.Matches(_latexString);

            foreach (Match match in matches)
            {
                //      grous:
                //  only one in each of 1&2 and 3&4 can be successful.
                //
                //  0:ignored
                //  1:  contains left
                //  2:  contains right
                //
                //  3:  contains parenthesis
                //  4:  contains abs bars

                var groups = match.Groups;

                // dealing with parenthesis
                if (groups[3].Success)
                {
                    _InsertToken(match.Index, new SepToken(MathSeparatorTokenTypes.Parenthesis, groups[2].Success));
                    continue;
                }

                // dealing with abs bars
                _InsertToken(match.Index, new SepToken(MathSeparatorTokenTypes.AbsoluteValue, groups[2].Success));
            }

            _WhiteOutString(matches);

            // -------------------------------------------------------------------------------------------------
            matches = ReExpoOp.Matches(_latexString);

            foreach (Match match in matches)
            {
                var groups = match.Groups;

                _InsertToken(groups[1].Index, new OpToken(MathOperatorTokenTypes.Expo));
            }


            // here we white out each match, one group at a time, backwards

            for (var i = matches.Count - 1; i >= 0; i--)
            {
                var iMatch = matches[i];

                for (var i1 = iMatch.Groups.Count - 1; i1 >= 1; i1--)
                {
                    var iGroup = iMatch.Groups[i1];
                    _WhiteOutString(iGroup.Index, iGroup.Length);
                }
            }

            // -------------------------------------------------------------------------------------------------
            matches = ReFracOp.Matches(_latexString);

            foreach (Match match in matches)
                // group 4 corresponds to the second to last bracket

                _InsertToken(match.Groups[4].Index, new OpToken(MathOperatorTokenTypes.Frac));

            // here we white out each match, one group at a time, backwards

            for (var i = matches.Count - 1; i >= 0; i--)
            {
                var iMatch = matches[i];

                for (var i1 = iMatch.Groups.Count - 1; i1 >= 1; i1--)
                {
                    var iGroup = iMatch.Groups[i1];
                    _WhiteOutString(iGroup.Index, iGroup.Length);
                }
            }
        }

        private void _InsertToken(int index, IMathToken token)
        {
            _tokens.Add((index, token));
        }

        private void _WhiteOutString(MatchCollection matches)
        {
            for (var matchIndex = matches.Count - 1; matchIndex >= 0; matchIndex--)
            {
                var match = matches[matchIndex];

                _WhiteOutString(match.Index, match.Length);
            }
        }

        private void _WhiteOutString(int index, int length)
        {
            _latexString = _latexString[..index] + ' ' + _latexString[(index + length)..];

            length--;

            for (var i = 0; i < _tokens.Count; i++)
            {
                var tokenTuple = _tokens[i];

                if (tokenTuple.Item1 >= index + length)
                {
                    tokenTuple.Item1 -= length;
                    _tokens[i] = tokenTuple;
                }
            }
        }

        /// <summary>
        ///     Only does minor verification.
        ///     Filters coefficients to include a \cdot (multiplication op)
        ///     where they are normally implied.
        /// </summary>
        private void _VerifyAndFilter()
        {
            // verify latex tokens are supported

            var pattern = $@"\\(?!{string.Join('|', SupportedTokens)})(\w+)";

            foreach (Match verifyMatch in Regex.Matches(_latexString, pattern))
            {
                var supportedToken = verifyMatch.Groups[1].ToString();

                if (!SupportedTokens.Contains(supportedToken))
                    throw new InvalidLatexExpressionException(verifyMatch.Index, verifyMatch.Length,
                        $"Unsupported token, \"{supportedToken}\"");
            }

            // filter unary subtraction

            _latexString = ReFilterUnarySub.Replace(_latexString, @"0-");

            // add parenthesis inside expression blocks { }

            _latexString = ReFilterBracketParenthesis.Replace(_latexString, @"{\left( $1\right)}");

            // now we will insert multiplication operators where they are implied

            // _latexString = ReFilterImpliedMult1.Replace(_latexString, @"$1\cdot $2");

            _latexString = ReFilterImpliedMult2.Replace(_latexString, m =>
            {
                var filledGroups = new List<string>();

                for (var i = 1; i < m.Groups.Count; i++)
                {
                    var thisGroup = m.Groups[i];

                    if (thisGroup.Success) filledGroups.Add(thisGroup.Value);
                }

                return string.Join("\\cdot ", filledGroups);
                ;
            });
        }

        private void _SortTokens()
        {
            _tokens.Sort((tokenA, tokenB) =>
            {
                if (tokenA.Item1 == tokenB.Item1) return 0;

                return tokenA.Item1 > tokenB.Item1 ? 1 : -1;
            });

            _filteredTokens = _tokens.Select((tuple, i) => tuple.Item2).ToList();
        }
    }
}