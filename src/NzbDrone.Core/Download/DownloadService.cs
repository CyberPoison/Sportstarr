using System;
using NLog;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Common.TPL;
using NzbDrone.Core.Download.Clients;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Download
{
    public interface IDownloadService
    {
        void DownloadReport(RemoteEpisode remoteEpisode);
    }


    public class DownloadService : IDownloadService
    {
        private readonly IProvideDownloadClient _downloadClientProvider;
        private readonly IDownloadClientStatusService _downloadClientStatusService;
        private readonly IIndexerStatusService _indexerStatusService;
        private readonly IRateLimitService _rateLimitService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public DownloadService(IProvideDownloadClient downloadClientProvider,
                               IDownloadClientStatusService downloadClientStatusService,
                               IIndexerStatusService indexerStatusService,
                               IRateLimitService rateLimitService,
                               IEventAggregator eventAggregator,
                               Logger logger)
        {
            _downloadClientProvider = downloadClientProvider;
            _downloadClientStatusService = downloadClientStatusService;
            _indexerStatusService = indexerStatusService;
            _rateLimitService = rateLimitService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public void DownloadReport(RemoteEpisode remoteEpisode)
        {
            Ensure.That(remoteEpisode.Series, () => remoteEpisode.Series).IsNotNull();
            Ensure.That(remoteEpisode.Episodes, () => remoteEpisode.Episodes).HasItems();

            var downloadTitle = remoteEpisode.Release.Title;
            var downloadClient = _downloadClientProvider.GetDownloadClient(remoteEpisode.Release.DownloadProtocol);

            if (downloadClient == null)
            {
                throw new DownloadClientUnavailableException($"{remoteEpisode.Release.DownloadProtocol} Download client isn't configured yet");
            }

            // Limit grabs to 2 per second.
            if (remoteEpisode.Release.DownloadUrl.IsNotNullOrWhiteSpace() && !remoteEpisode.Release.DownloadUrl.StartsWith("magnet:"))
            {
                var url = new HttpUri(remoteEpisode.Release.DownloadUrl);
                _rateLimitService.WaitAndPulse(url.Host, TimeSpan.FromSeconds(2));
            }

            string downloadClientId;
            try
            {
                downloadClientId = downloadClient.Download(remoteEpisode);
                _downloadClientStatusService.RecordSuccess(downloadClient.Definition.Id);
                _indexerStatusService.RecordSuccess(remoteEpisode.Release.IndexerId);
            }
            catch (ReleaseUnavailableException)
            {
                _logger.Trace("Release {0} no longer available on indexer.", remoteEpisode);
                throw;
            }
            catch (ReleaseDownloadException ex)
            {
                var http429 = ex.InnerException as TooManyRequestsException;
                if (http429 != null)
                {
                    _indexerStatusService.RecordFailure(remoteEpisode.Release.IndexerId, http429.RetryAfter);
                }
                else
                {
                    _indexerStatusService.RecordFailure(remoteEpisode.Release.IndexerId);
                }
                throw;
            }

            var episodeGrabbedEvent = new EpisodeGrabbedEvent(remoteEpisode);
            episodeGrabbedEvent.DownloadClient = downloadClient.GetType().Name;

            if (!string.IsNullOrWhiteSpace(downloadClientId))
            {
                episodeGrabbedEvent.DownloadId = downloadClientId;
            }

            _logger.ProgressInfo("Report sent to {0}. {1}", downloadClient.Definition.Name, downloadTitle);
            _eventAggregator.PublishEvent(episodeGrabbedEvent);
        }
    }
}
