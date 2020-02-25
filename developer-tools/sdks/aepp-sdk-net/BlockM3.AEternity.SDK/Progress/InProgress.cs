using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BlockM3.AEternity.SDK.Progress
{
    public class InProgress<T>
    {
        private readonly List<ITransactionProgress> _chains;
        private int _index;

        private object _input;

        public InProgress(ITransactionProgress t, params ITransactionProgress[] chains)
        {
            List<ITransactionProgress> ch = new List<ITransactionProgress> {t};
            if (chains != null)
                ch.AddRange(chains);
            _chains = ch;
        }

        public string TxHash => _chains.First(a => a.TXHash != null).TXHash;


        private T ReturnValue(object l)
        {
            if (l is T)
                return (T) l;
            if (typeof(T).IsAssignableFrom(typeof(bool)))
                return (T) (object) true;
            return default(T);
        }


        private async Task<(object result, bool state)> InternalCheckForFinish(CancellationToken token = default(CancellationToken))
        {
            (object result, bool done) r;
            do
            {
                ITransactionProgress p = _chains[_index];
                r = await p.CheckForFinishAsync(_input, token).ConfigureAwait(false);
                if (r.done)
                {
                    _index++;
                    if (_chains.Count == _index)
                        return r;
                    _input = r.result;
                }
            } while (r.done);

            return r;
        }

        public async Task<T> CheckForFinishAsync(CancellationToken token = default(CancellationToken))
        {
            (object result, bool done) r = await InternalCheckForFinish(token).ConfigureAwait(false);
            if (r.done)
                return ReturnValue(r.result);
            return default(T);
        }

        public async Task<T> WaitForFinishAsync(TimeSpan span, CancellationToken token = default(CancellationToken))
        {
            Stopwatch w = new Stopwatch();
            w.Start();
            while (w.ElapsedTicks < span.Ticks)
            {
                (object result, bool done) r = await InternalCheckForFinish(token).ConfigureAwait(false);
                if (r.done)
                {
                    w.Stop();
                    return ReturnValue(r.result);
                }

                if (token.IsCancellationRequested)
                    token.ThrowIfCancellationRequested();
                await Task.Delay(1000, token).ConfigureAwait(false);
                if (token.IsCancellationRequested)
                    token.ThrowIfCancellationRequested();
            }

            w.Stop();
            throw new TimeoutException("Unable to get the transaction in the desired time, elapsed time: " + span);
        }

        public async Task<T> WaitForFinishAsync(CancellationToken token = default(CancellationToken))
        {
            while (true)
            {
                (object result, bool done) r = await InternalCheckForFinish(token).ConfigureAwait(false);
                if (r.done)
                    return ReturnValue(r.result);
                if (token.IsCancellationRequested)
                    token.ThrowIfCancellationRequested();
                await Task.Delay(1000, token).ConfigureAwait(false);
                if (token.IsCancellationRequested)
                    token.ThrowIfCancellationRequested();
            }
        }
    }
}