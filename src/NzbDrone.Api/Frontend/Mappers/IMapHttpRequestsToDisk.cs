﻿
using Nancy;

namespace NzbDrone.Api.Frontend.Mappers
{
    public interface IMapHttpRequestsToDisk
    {
        string Map(string resourceUrl);
        bool CanHandle(string resourceUrl);
        Response GetResponse(string resourceUrl);
    }
}
