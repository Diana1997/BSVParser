namespace BSVParser.Models;

public class TransactionInput
{
    public string PreviousTransactionId { get; set; } = string.Empty;
    public uint Vout { get; set; }
    public string ScriptSig { get; set; } = string.Empty;
    public uint Sequence { get; set; }
}