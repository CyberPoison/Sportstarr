﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.DiskSpace
{
    public interface IDiskSpaceService
    {
        List<DiskSpace> GetFreeSpace();
    }

    public class DiskSpaceService : IDiskSpaceService
    {
        private readonly ISeriesService _seriesService;
        private readonly IConfigService _configService;
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        private static readonly Regex _regexSpecialDrive = new Regex("^/var/lib/(docker|rancher|kubelet)(/|$)|^/boot(/|$)|/docker(/var)?/aufs(/|$)", RegexOptions.Compiled);

        public DiskSpaceService(ISeriesService seriesService, IConfigService configService, IDiskProvider diskProvider, Logger logger)
        {
            _seriesService = seriesService;
            _configService = configService;
            _diskProvider = diskProvider;
            _logger = logger;
        }

        public List<DiskSpace> GetFreeSpace()
        {
            var importantRootFolders = GetSeriesRootPaths().Concat(GetDroneFactoryRootPaths()).Distinct().ToList();

            var optionalRootFolders = GetFixedDisksRootPaths().Except(importantRootFolders).Distinct().ToList();

            var diskSpace = GetDiskSpace(importantRootFolders).Concat(GetDiskSpace(optionalRootFolders, true)).ToList();

            return diskSpace;
        }

        private IEnumerable<string> GetSeriesRootPaths()
        {
            return _seriesService.GetAllSeries()
                .Where(s => _diskProvider.FolderExists(s.Path))
                .Select(s => _diskProvider.GetPathRoot(s.Path))
                .Distinct();
        }

        private IEnumerable<string> GetDroneFactoryRootPaths()
        {
            if (_configService.DownloadedEpisodesFolder.IsNotNullOrWhiteSpace() && _diskProvider.FolderExists(_configService.DownloadedEpisodesFolder))
            {
                yield return _configService.DownloadedEpisodesFolder;
            }
        }

        private IEnumerable<string> GetFixedDisksRootPaths()
        {
            return _diskProvider.GetMounts()
                .Where(d => d.DriveType == DriveType.Fixed)
                .Where(d => !_regexSpecialDrive.IsMatch(d.RootDirectory))
                .Select(d => d.RootDirectory);
        }

        private IEnumerable<DiskSpace> GetDiskSpace(IEnumerable<string> paths, bool suppressWarnings = false)
        {
            foreach (var path in paths)
            {
                DiskSpace diskSpace = null;

                try
                {
                    var freeSpace = _diskProvider.GetAvailableSpace(path);
                    var totalSpace = _diskProvider.GetTotalSize(path);

                    if (!freeSpace.HasValue || !totalSpace.HasValue)
                    {
                        continue;
                    }

                    diskSpace = new DiskSpace
                                {
                                    Path = path,
                                    FreeSpace = freeSpace.Value,
                                    TotalSpace = totalSpace.Value
                                };

                    diskSpace.Label = _diskProvider.GetVolumeLabel(path);
                }
                catch (Exception ex)
                {
                    if (!suppressWarnings)
                    {
                        _logger.Warn(ex, "Unable to get free space for: " + path);
                    }
                }

                if (diskSpace != null)
                {
                    yield return diskSpace;
                }
            }
        }
    }
}
