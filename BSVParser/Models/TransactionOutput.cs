namespace BSVParser.Models;

public class TransactionOutput
{
    public ulong Value { get; set; }
    public string ScriptPubKey { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}