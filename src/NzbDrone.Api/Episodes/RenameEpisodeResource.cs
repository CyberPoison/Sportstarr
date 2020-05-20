﻿using System.Collections.Generic;
using System.Linq;
using NzbDrone.Api.REST;

namespace NzbDrone.Api.Episodes
{
    public class RenameEpisodeResource : RestResource
    {
        public int SeriesId { get; set; }
        public int SeasonNumber { get; set; }
        public List<int> EpisodeNumbers { get; set; }
        public int EpisodeFileId { get; set; }
        public string ExistingPath { get; set; }
        public string NewPath { get; set; }
    }

    public static class RenameEpisodeResourceMapper
    {
        public static RenameEpisodeResource ToResource(this Core.MediaFiles.RenameEpisodeFilePreview model)
        {
            if (model == null) return null;

            return new RenameEpisodeResource
            {
                SeriesId = model.SeriesId,
                SeasonNumber = model.SeasonNumber,
                EpisodeNumbers = model.EpisodeNumbers.ToList(),
                EpisodeFileId = model.EpisodeFileId,
                ExistingPath = model.ExistingPath,
                NewPath = model.NewPath
            };
        }

        public static List<RenameEpisodeResource> ToResource(this IEnumerable<Core.MediaFiles.RenameEpisodeFilePreview> models)
        {
            return models.Select(ToResource).ToList();
        }
    }
}
