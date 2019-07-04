﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FTServer.Monitor
{
    /// <summary>
    /// A metric server that regularly pushes metrics to a Prometheus PushGateway.
    /// </summary>
    public class MetricPusher : MetricHandler
    {
        private readonly TimeSpan _pushInterval;
        private readonly Uri _targetUrl;

        public MetricPusher(string endpoint, string job, string instance = null, long intervalMilliseconds = 1000, IEnumerable<Tuple<string, string>> additionalLabels = null, CollectorRegistry registry = null) : base(registry)
        {
            if (string.IsNullOrEmpty(endpoint))
            {
                throw new ArgumentNullException("endpoint");
            }
            if (string.IsNullOrEmpty(job))
            {
                throw new ArgumentNullException("job");
            }
            if (intervalMilliseconds <= 0)
            {
                throw new ArgumentException("Interval must be greater than zero", "intervalMilliseconds");
            }

            StringBuilder sb = new StringBuilder(string.Format("{0}/job/{1}", endpoint.TrimEnd('/'), job));
            if (!string.IsNullOrEmpty(instance))
            {
                sb.AppendFormat("/instance/{0}", instance);
            }

            if (additionalLabels != null)
            {
                foreach (var pair in additionalLabels)
                {
                    if (pair == null || string.IsNullOrEmpty(pair.Item1) || string.IsNullOrEmpty(pair.Item2))
                        throw new NotSupportedException($"Invalid {nameof(MetricPusher)} additional label: ({pair?.Item1}):({pair?.Item2})");

                    sb.AppendFormat("/{0}/{1}", pair.Item1, pair.Item2);
                }
            }

            if (!Uri.TryCreate(sb.ToString(), UriKind.Absolute, out _targetUrl))
            {
                throw new ArgumentException("Endpoint must be a valid url", "endpoint");
            }

            _pushInterval = TimeSpan.FromMilliseconds(intervalMilliseconds);
        }

        private static readonly HttpClient _httpClient = new HttpClient();

        protected override Task StartServer(CancellationToken cancel)
        {
            // Kick off the actual processing to a new thread and return a Task for the processing thread.
            return Task.Run(async delegate
            {
                while (true)
                {
                    // We schedule approximately at the configured interval. There may be some small accumulation for the
                    // part of the loop we do not measure but it is close enough to be acceptable for all practical scenarios.
                    var duration = Stopwatch.StartNew();
                    try
                    {
                        using (var stream = new MemoryStream())
                        {
                            var serializer = new TextSerializer(stream);

                            // Do not pass CT because we only want to cancel after pushing, so a flush is always performed.
                            await _registry.CollectAndSerializeAsync(serializer, default);

                            stream.Position = 0;
                            // StreamContent takes ownership of the stream.
                            var response = await _httpClient.PostAsync(_targetUrl, new StreamContent(stream));

                            // If anything goes wrong, we want to get at least an entry in the trace log.
                            response.EnsureSuccessStatusCode();
                        }
                    }
                    catch (ScrapeFailedException ex)
                    {
                        LogProxy.WriteLine($"Skipping metrics push due to failed scrape: {ex.Message}");
                    }
                    catch (Exception ex) when (!(ex is OperationCanceledException))
                    {
                        LogProxy.WriteLine(string.Format("Error in MetricPusher: {0}", ex));
                    }

                    // We stop only after pushing metrics, to ensure that the latest state is flushed when told to stop.
                    if (cancel.IsCancellationRequested)
                        break;
                    var sleepTime = _pushInterval - duration.Elapsed;

                    // Sleep until the interval elapses or the pusher is asked to shut down.
                    if (sleepTime > TimeSpan.Zero)
                    {
                        try
                        {
                            await Task.Delay(sleepTime, cancel);
                        }
                        catch (OperationCanceledException)
                        {
                            // The task was cancelled.
                            // We continue the loop here to ensure final state gets pushed.
                            continue;
                        }
                    }
                }
            });
        }
    }
}
