﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using umbraco;

namespace Umbraco.Core.IO
{
    public static class IMediaFileSystemExtensions
    {
        internal static string GetRelativePath(this IMediaFileSystem fs, int propertyId, string fileName)
        {
            var seperator = UmbracoSettings.UploadAllowDirectories
                ? Path.DirectorySeparatorChar 
                : '-';

            return propertyId.ToString() + seperator + fileName;
        }

        internal static IEnumerable<string> GetThumbnails(this IMediaFileSystem fs, string path)
        {
            var parentDirectory = System.IO.Path.GetDirectoryName(path);
            var extension = System.IO.Path.GetExtension(path);

            return fs.GetFiles(parentDirectory)
                .Where(x => x.StartsWith(path.TrimEnd(extension) + "_thumb"))
                .ToList();
        }

        internal static void DeleteFile(this IMediaFileSystem fs, string path, bool deleteThumbnails)
        {
            fs.DeleteFile(path);

            if(!deleteThumbnails)
                return;

            fs.DeleteThumbnails(path);
        }

        internal static void DeleteThumbnails(this IMediaFileSystem fs, string path)
        {
            fs.GetThumbnails(path)
                .ForEach(fs.DeleteFile);
        }
    }
}
