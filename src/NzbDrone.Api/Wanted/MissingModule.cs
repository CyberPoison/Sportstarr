﻿using NzbDrone.Api.Episodes;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Tv;
using NzbDrone.SignalR;

namespace NzbDrone.Api.Wanted
{
    public class MissingModule : EpisodeModuleWithSignalR
    {
        public MissingModule(IEpisodeService episodeService,
                             ISeriesService seriesService,
                             IQualityUpgradableSpecification qualityUpgradableSpecification,
                             IBroadcastSignalRMessage signalRBroadcaster)
            : base(episodeService, seriesService, qualityUpgradableSpecification, signalRBroadcaster, "wanted/missing")
        {
            GetResourcePaged = GetMissingEpisodes;
        }

        private PagingResource<EpisodeResource> GetMissingEpisodes(PagingResource<EpisodeResource> pagingResource)
        {
            var pagingSpec = pagingResource.MapToPagingSpec<EpisodeResource, Episode>("airDateUtc", SortDirection.Descending);

            if (pagingResource.FilterKey == "monitored" && pagingResource.FilterValue == "false")
            {
                pagingSpec.FilterExpression = v => v.Monitored == false || v.Series.Monitored == false;
            }
            else
            {
                pagingSpec.FilterExpression = v => v.Monitored == true && v.Series.Monitored == true;
            }

            var resource = ApplyToPage(_episodeService.EpisodesWithoutFiles, pagingSpec, v => MapToResource(v, true, false));

            return resource;
        }
    }
}
