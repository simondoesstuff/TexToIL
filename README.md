# LaTeX -> IL Code

*LatexToIL* is a parser for math expressions in LaTeX form with support for variables and trancendental functions. Expressions are lexed, parsed, and assembled into native c# expression trees which compile to IL code. Compiled expressions can be executed with differing parameters in rapid succession without reparsing the entire string.