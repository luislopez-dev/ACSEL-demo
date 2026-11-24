using Oracle.ManagedDataAccess.Client;

namespace AcselDemo.Exceptions;

public class OracleBusinessRuleException : Exception
{
    public int OracleErrorCode { get; }

    public OracleBusinessRuleException(int oracleErrorCode, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        OracleErrorCode = oracleErrorCode;
    }

    public static bool TryTranslate(OracleException ex, out OracleBusinessRuleException? businessException)
    {
        if (ex.Number is >= 20000 and <= 20999)
        {
            var rawLine = ex.Message
                .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault() ?? ex.Message;

            var prefix = $"ORA-{ex.Number}:";
            var cleanMessage = rawLine.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                ? rawLine[prefix.Length..].Trim()
                : rawLine.Trim();

            businessException = new OracleBusinessRuleException(ex.Number, cleanMessage, ex);
            return true;
        }

        businessException = null;
        return false;
    }
}
