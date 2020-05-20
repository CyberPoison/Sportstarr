﻿using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.TvTests.SeriesServiceTests
{
    [TestFixture]
    public class UpdateMultipleSeriesFixture : CoreTest<SeriesService>
    {
        private List<Series> _series;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateListOfSize(5)
                .All()
                .With(s => s.ProfileId = 1)
                .With(s => s.Monitored)
                .With(s => s.SeasonFolder)
                .With(s => s.Path = @"C:\Test\name".AsOsAgnostic())
                .With(s => s.RootFolderPath = "")
                .Build().ToList();
        }

        [Test]
        public void should_call_repo_updateMany()
        {
            Subject.UpdateSeries(_series);

            Mocker.GetMock<ISeriesRepository>().Verify(v => v.UpdateMany(_series), Times.Once());
        }

        [Test]
        public void should_update_path_when_rootFolderPath_is_supplied()
        {
            var newRoot = @"C:\Test\TV2".AsOsAgnostic();
            _series.ForEach(s => s.RootFolderPath = newRoot);

            Subject.UpdateSeries(_series).ForEach(s => s.Path.Should().StartWith(newRoot));
        }

        [Test]
        public void should_not_update_path_when_rootFolderPath_is_empty()
        {
            Subject.UpdateSeries(_series).ForEach(s =>
            {
                var expectedPath = _series.Single(ser => ser.Id == s.Id).Path;
                s.Path.Should().Be(expectedPath);
            });
        }

        [Test]
        public void should_be_able_to_update_many_series()
        {
            var series = Builder<Series>.CreateListOfSize(50)
                                        .All()
                                        .With(s => s.Path = (@"C:\Test\TV\" + s.Path).AsOsAgnostic())
                                        .Build()
                                        .ToList();

            var newRoot = @"C:\Test\TV2".AsOsAgnostic();
            series.ForEach(s => s.RootFolderPath = newRoot);

            Subject.UpdateSeries(series);
        }
    }
}