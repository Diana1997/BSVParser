namespace BSVParser.Models;

public class Transaction
{
    public string TransactionId { get; set; } = string.Empty;
    public uint Version { get; set; }
    public IList<TransactionInput> Inputs { get; set; } = [];
    public IList<TransactionOutput> Outputs { get; set; } = [];
    public uint LockTime { get; set; }
}