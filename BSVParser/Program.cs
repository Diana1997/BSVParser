
using BSVParser.Services;

Console.WriteLine("Enter transaction");
var hexTransaction = Console.ReadLine();

if (hexTransaction != null)
{
    var rawTx = Convert.FromHexString(hexTransaction);
    var tx = TransactionParserService.Parse(rawTx);

    Console.WriteLine($"Transaction ID: {tx.TransactionId}");
    foreach (var input in tx.Inputs)
    {
        Console.WriteLine($"Input: PrevTxId={input.PreviousTransactionId}, Vout={input.Vout}");
    }
    foreach (var output in tx.Outputs)
    {
        Console.WriteLine($"Output: Address={output.Address}, Value={output.Value} satoshis");
    }
}