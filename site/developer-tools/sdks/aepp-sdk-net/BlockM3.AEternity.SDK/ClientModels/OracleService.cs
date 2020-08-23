using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BlockM3.AEternity.SDK.Extensions;
using BlockM3.AEternity.SDK.Progress;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace BlockM3.AEternity.SDK.ClientModels
{
    public abstract class OracleService<T, S> : AsyncOracleService<T, S>
    {
        public OracleService(ILogger logger) : base(logger)
        {
        }

        public OracleService()
        {
        }

        public abstract S Answer(T ask);

        public override Task<S> AnswerAsync(T ask, CancellationToken token = default(CancellationToken))
        {
            return Task.FromResult(Answer(ask));
        }

        public void Start() => StartAsync(CancellationToken.None).RunAndUnwrap();
        public void Stop() => StopAsync(CancellationToken.None).RunAndUnwrap();
    }

    public abstract class AsyncOracleService<T, S> : BackgroundService
    {
        private readonly ILogger _logger;

        private readonly Dictionary<InProgress<bool>, WaitCount> _responses = new Dictionary<InProgress<bool>, WaitCount>();

        public AsyncOracleService(ILogger logger)
        {
            _logger = logger;
        }

        public AsyncOracleService()
        {
            _logger = NullLoggerFactory.Instance.CreateLogger<AsyncOracleService<T, S>>();
        }

        public OracleServer<T, S> Server { get; set; }

        public ulong RespondTtl { get; set; } = Constants.BaseConstants.ORACLE_RESPONSE_TTL_VALUE;

        public int MaxRetries { get; set; } = 3;

        public TimeSpan TransactionTimeout { get; set; } = new TimeSpan(0, 1, 0);

        public abstract Task<S> AnswerAsync(T ask, CancellationToken token = default(CancellationToken));

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Oracle Service Started");

            while (!stoppingToken.IsCancellationRequested)
            {
                if (Server != null)
                {
                    List<OracleQuestion<T, S>> q = await Server.QueryAsync(10, null, stoppingToken).ConfigureAwait(false);
                    foreach (OracleQuestion<T, S> m in q)
                    {
                        int cnt = 0;
                        bool done = false;
                        InProgress<bool> res = null;
                        do
                        {
                            try
                            {
                                S result = await AnswerAsync(m.Query, stoppingToken).ConfigureAwait(false);
                                res = await m.RespondAsync(result, RespondTtl, stoppingToken).ConfigureAwait(false);
                                done = true;
                            }
                            catch (Exception e)
                            {
                                cnt++;
                                if (cnt == MaxRetries)
                                {
                                    _logger.LogError($"Error responding QueryId {m.Id} after {MaxRetries} retries: {e.Message}");
                                    done = true;
                                    res = null;
                                }
                                else
                                    _logger.LogWarning($"Error responding QueryId {m.Id} ({cnt}/{MaxRetries}): {e.Message}");
                            }
                        } while (!done);

                        if (res != null)
                            _responses.Add(res, new WaitCount(m.Id, TransactionTimeout));
                    }
                }

                await Task.Delay(1000, stoppingToken).ConfigureAwait(false);
                foreach (InProgress<bool> n in _responses.Keys.ToList())
                {
                    if (await n.CheckForFinishAsync(stoppingToken).ConfigureAwait(false))
                        _responses.Remove(n);
                    else
                    {
                        if (_responses[n].Timeout > DateTime.UtcNow)
                        {
                            _logger.LogError($"Error waiting for response transaction Hash:{n.TxHash} of QueryId:{_responses[n].QueryId} after {TransactionTimeout.TotalSeconds} seconds");
                            _responses.Remove(n);
                        }
                    }
                }
            }

            _logger.LogInformation("Oracle Service Stopped");
        }

        public class WaitCount
        {
            public string QueryId;
            public DateTime Timeout;

            public WaitCount(string queryId, TimeSpan timeout)
            {
                Timeout = DateTime.UtcNow.Add(timeout);
                QueryId = queryId;
            }
        }
    }
}