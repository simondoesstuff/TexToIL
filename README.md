# LaTeX -> IL Code

*LatexToIL* is a parser for math expressions in LaTeX form with support for variables and trancendental functions. Expressions are lexed, parsed, and assembled into native c# expression trees which compile to IL code. Compiled expressions can be executed with differing parameters in rapid succession without reparsing the entire string.

# The Mission

https://www.desmos.com/ is an online graphing calculator aimed to make *"math beautiful"*. Desmos depends on an excellent resource called **Math Quill** which allows users to easily type LaTeX math with their keyboard and render it in realtime. **The mission of this project** is to bring the beauty of desmos to procedural terrain generation.

Procedural terrain generation is a highly mathematical process. *LatexToIL* provides an API to compile LaTeX math expressions into IL code so terrain can be computed and animated in realtime. It has support for standard expressions like `3 \sqrt{ 35 }`; for variables, defined by a unique letter except for `n` which is pre-defined to represent the layer number; and for noise functions, represented as `p(x)`. All noise functions are implemented in 2 dimensions so mathematical transforms on `x` in `p(x)` are performed on x and y internally. Eg: `9 p(.3x)` is interpreted internally as `9` `*` PerlinNoise(`.3` `*` `x`, `.3` `*` `y`).
