using System.Linq;
using NzbDrone.Common.Disk;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.HealthCheck.Checks
{
    [CheckOn(typeof(SeriesDeletedEvent))]
    [CheckOn(typeof(SeriesMovedEvent))]
    [CheckOn(typeof(EpisodeImportedEvent), CheckOnCondition.FailedOnly)]
    [CheckOn(typeof(EpisodeImportFailedEvent), CheckOnCondition.SuccessfulOnly)]
    public class RootFolderCheck : HealthCheckBase
    {
        private readonly ISeriesService _seriesService;
        private readonly IDiskProvider _diskProvider;

        public RootFolderCheck(ISeriesService seriesService, IDiskProvider diskProvider)
        {
            _seriesService = seriesService;
            _diskProvider = diskProvider;
        }

        public override HealthCheck Check()
        {
            var missingRootFolders = _seriesService.GetAllSeries()
                                                   .Select(s => _diskProvider.GetParentFolder(s.Path))
                                                   .Distinct()
                                                   .Where(s => !_diskProvider.FolderExists(s))
                                                   .ToList();

            if (missingRootFolders.Any())
            {
                if (missingRootFolders.Count == 1)
                {
                    return new HealthCheck(GetType(), HealthCheckResult.Error, "Missing root folder: " + missingRootFolders.First(), "#missing-root-folder");
                }

                var message = string.Format("Multiple root folders are missing: {0}", string.Join(" | ", missingRootFolders));
                return new HealthCheck(GetType(), HealthCheckResult.Error, message, "#missing-root-folder");
            }

            return new HealthCheck(GetType());
        }
    }
}
