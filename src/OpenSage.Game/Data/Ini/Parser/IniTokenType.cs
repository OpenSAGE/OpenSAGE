namespace OpenSage.Data.Ini.Parser
{
    internal enum IniTokenType
    {
        Identifier,
        Equals,
        IntegerLiteral,
        FloatLiteral,
        PercentLiteral,
        StringLiteral,
        Colon,
        Comma,
        EndOfLine,
        EndOfFile
    }
}
