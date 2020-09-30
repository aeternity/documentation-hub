using System.Threading;
using System.Threading.Tasks;

namespace BlockM3.AEternity.SDK.Progress
{
    public interface ITransactionProgress
    {
        string TXHash { get; }
        Task<(object result, bool done)> CheckForFinishAsync(object input, CancellationToken token = default(CancellationToken));
    }
}