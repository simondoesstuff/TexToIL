# The Mission

https://www.desmos.com/ is an online graphing calculator aimed to make *"math beautiful"*. Desmos depends on an excellent resource called **Math Quill** which allows users to easily type LaTeX math with their keyboard and render it in realtime. **The mission of this project** is to bring the beauty of desmos to procedural terrain generation.

Procedural Terrain Generation is a highly mathematicaly process. *LatexProcessing* provides an API to compile LaTeX math expressions into IL code so terrain can be computed and animated in realtime. It has support for standard expressions like `3 \sqrt{ 35 }`; for variables, defined by a unique letter except for `n` which is pre-defined to represent the layer number; and for noise functions, represented as `p(x)` (eg: `9 p(.3x)`) which is interpreted internally as `9` `*` PerlinNoise(`.3x`, `.3y`, `.3z`).
