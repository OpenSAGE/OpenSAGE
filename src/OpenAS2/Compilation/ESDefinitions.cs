using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAS2.Compilation
{
    public static class ESDefinitions
    {

        static ESDefinitions()
        {
            var keywords51 = new string[]
            {
                "break",
                "do",
                "instanceof",
                "typeof",
                "case",
                "else",
                "new",
                "var",
                "catch",
                "finally",
                "return",
                "void",
                "continue",
                "for",
                "switch",
                "while",
                "debugger",
                "function",
                "this",
                "with",
                "default",
                "if",
                "throw",
                "delete",
                "in",
                "try"
            };
            var keywords12 = new string[]
            {
                "await", "break", "case", "catch", "class", "const", "continue", "debugger", "default",
                "delete", "do", "else", "enum", "export", "extends", "false", "finally", "for",
                "function", "if", "import", "in", "instanceof", "new", "null", "return", "super",
                "switch", "this", "throw", "true", "try", "typeof", "var", "void", "while", "with", "yield"
            };
            var reserve51 = new string[]
            {
                "class", "enum", "extends", "super", "const", "export", "import"
            };
            var reserve12 = new string[]
            {
                "enum", "implements", "interface", "package", "private", "protected", "public"
            };
            var reserveStrict51 = new string[]
            {
                "implements", "let", "private", "public", "yield", "interface", "package", "protected", "static"
            };
            var reserveStrict12 = new string[]
            {
                "arguments", "eval"
            };
        }

    }
}
