﻿using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Api.Indexers;
using System.Linq;
using System.Net;
using System.Collections.Generic;
using System;
using System.Globalization;

namespace NzbDrone.Integration.Test.ApiTests
{
    [TestFixture]
    public class ReleasePushFixture : IntegrationTest
    {
        [Test]
        public void should_have_utc_date()
        {
            var body = new Dictionary<string, object>();
            body.Add("guid", "sdfsdfsdf");
            body.Add("title", "The.Series.S01E01");
            body.Add("publishDate", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ssZ", CultureInfo.InvariantCulture));

            var request = ReleasePush.BuildRequest();
            request.AddBody(body);
            var result = ReleasePush.Post<ReleaseResource>(request, HttpStatusCode.OK);

            result.Should().NotBeNull();
            result.AgeHours.Should().BeApproximately(0, 0.1);
        }
    }
}
