namespace OpenSage.Data.Ini
{
    internal enum IniTokenType
    {
        Identifier,
        Equals,
        IntegerLiteral,
        LongLiteral,
        FloatLiteral,
        Percent,
        StringLiteral,
        Colon,
        Comma,
        LeftParen,
        RightParen,
        DefineKeyword,
        IncludeKeyword,
        EndOfLine,
        EndOfFile
    }
}
