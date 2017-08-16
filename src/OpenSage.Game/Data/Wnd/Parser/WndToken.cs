namespace OpenSage.Data.Wnd.Parser
{
    internal struct WndToken
    {
        public WndTokenType TokenType;
        public string StringValue;
        public int IntegerValue;

        public WndToken(WndTokenType tokenType)
        {
            TokenType = tokenType;
            StringValue = null;
            IntegerValue = int.MinValue;
        }

        public override string ToString()
        {
            return $"Type: {TokenType}; StringValue: {StringValue}; IntegerValue: {IntegerValue}";
        }
    }
}
