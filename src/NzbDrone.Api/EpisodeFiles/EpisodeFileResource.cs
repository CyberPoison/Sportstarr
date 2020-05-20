using System;
using System.IO;
using NzbDrone.Api.REST;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Api.EpisodeFiles
{
    public class EpisodeFileResource : RestResource
    {
        public int SeriesId { get; set; }
        public int SeasonNumber { get; set; }
        public string RelativePath { get; set; }
        public string Path { get; set; }
        public long Size { get; set; }
        public DateTime DateAdded { get; set; }
        public string SceneName { get; set; }
        public QualityModel Quality { get; set; }
        public MediaInfoResource MediaInfo { get; set; }

        public bool QualityCutoffNotMet { get; set; }
    }

    public static class EpisodeFileResourceMapper
    {
        private static EpisodeFileResource ToResource(this Core.MediaFiles.EpisodeFile model)
        {
            if (model == null) return null;

            return new EpisodeFileResource
            {
                Id = model.Id,

                SeriesId = model.SeriesId,
                SeasonNumber = model.SeasonNumber,
                RelativePath = model.RelativePath,
                //Path
                Size = model.Size,
                DateAdded = model.DateAdded,
                SceneName = model.SceneName,
                Quality = model.Quality,
                MediaInfo = model.MediaInfo.ToResource(model.SceneName)
                //QualityCutoffNotMet
            };
        }

        public static EpisodeFileResource ToResource(this Core.MediaFiles.EpisodeFile model, Core.Tv.Series series, Core.DecisionEngine.IQualityUpgradableSpecification qualityUpgradableSpecification)
        {
            if (model == null) return null;

            return new EpisodeFileResource
            {
                Id = model.Id,

                SeriesId = model.SeriesId,
                SeasonNumber = model.SeasonNumber,
                RelativePath = model.RelativePath,
                Path = Path.Combine(series.Path, model.RelativePath),
                Size = model.Size,
                DateAdded = model.DateAdded,
                SceneName = model.SceneName,
                Quality = model.Quality,
                QualityCutoffNotMet = qualityUpgradableSpecification.CutoffNotMet(series.Profile.Value, model.Quality),
                MediaInfo = model.MediaInfo.ToResource(model.SceneName),
            };
        }
    }
}
