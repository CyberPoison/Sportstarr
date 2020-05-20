using System.Collections.Generic;
using System.Linq;
using Nancy;
using NzbDrone.Api.Extensions;
using NzbDrone.Core.Tv;

namespace NzbDrone.Api.Series
{
    public class SeriesEditorModule : NzbDroneApiModule
    {
        private readonly ISeriesService _seriesService;

        public SeriesEditorModule(ISeriesService seriesService)
            : base("/series/editor")
        {
            _seriesService = seriesService;
            Put["/"] = series => SaveAll();
        }

        private Response SaveAll()
        {
            var resources = Request.Body.FromJson<List<SeriesResource>>();

            var series = resources.Select(seriesResource => seriesResource.ToModel(_seriesService.GetSeries(seriesResource.Id))).ToList();

            return _seriesService.UpdateSeries(series)
                                 .ToResource(false)
                                 .AsResponse(HttpStatusCode.Accepted);
        }
    }
}
