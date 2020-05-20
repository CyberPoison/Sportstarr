﻿using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.History;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Specifications
{
    public class GrabbedReleaseQualitySpecification : IImportDecisionEngineSpecification
    {
        private readonly IHistoryService _historyService;
        private readonly Logger _logger;

        public GrabbedReleaseQualitySpecification(IHistoryService historyService, Logger logger)
        {
            _historyService = historyService;
            _logger = logger;
        }
        public Decision IsSatisfiedBy(LocalEpisode localEpisode, DownloadClientItem downloadClientItem)
        {
            if (downloadClientItem == null)
            {
                _logger.Debug("No download client item provided, skipping.");
                return Decision.Accept();
            }

            var grabbedHistory = _historyService.FindByDownloadId(downloadClientItem.DownloadId)
                                              .Where(h => h.EventType == HistoryEventType.Grabbed)
                                              .ToList();

            if (grabbedHistory.Empty())
            {
                _logger.Debug("No grabbed history for this download client item");
                return Decision.Accept();
            }

            var parsedReleaseName = Parser.Parser.ParseTitle(grabbedHistory.First().SourceTitle);

            if (parsedReleaseName != null && parsedReleaseName.FullSeason)
            {
                _logger.Debug("File is part of a season pack, skipping.");
                return Decision.Accept();
            }

            foreach (var item in grabbedHistory)
            {
                if (item.Quality.Quality != Quality.Unknown && item.Quality != localEpisode.Quality)
                {
                    _logger.Debug("Quality for grabbed release ({0}) does not match the quality of the file ({1})", item.Quality, localEpisode.Quality);
                    return Decision.Reject("File quality does not match quality of the grabbed release");
                }
            }

            return Decision.Accept();
        }
    }
}
