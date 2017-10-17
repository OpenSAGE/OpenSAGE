namespace OpenSage.Data.Ini.Parser
{
    internal enum IniTokenType
    {
        Identifier,
        Equals,
        IntegerLiteral,
        LongLiteral,
        FloatLiteral,
        PercentLiteral,
        StringLiteral,
        Colon,
        Comma,
        DefineKeyword,
        EndOfLine,
        EndOfFile
    }
}
