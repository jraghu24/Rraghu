﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Umbraco.Core.CodeAnnotations;
using Umbraco.Core.Configuration;

namespace Umbraco.Core.IO
{
	/// <summary>
	/// A custom file system provider for media
	/// </summary>
	[FileSystemProvider("media")]
	[UmbracoExperimentalFeature("http://issues.umbraco.org/issue/U4-1156", "Will be declared public after 4.10")]
	internal class MediaFileSystem : FileSystemWrapper
	{
		public MediaFileSystem(IFileSystem wrapped)
			: base(wrapped)
		{
		}

		public string GetRelativePath(int propertyId, string fileName)
		{
			var seperator = UmbracoSettings.UploadAllowDirectories
				? Path.DirectorySeparatorChar
				: '-';

			return propertyId.ToString() + seperator + fileName;
		}

		public IEnumerable<string> GetThumbnails(string path)
		{
			var parentDirectory = System.IO.Path.GetDirectoryName(path);
			var extension = System.IO.Path.GetExtension(path);

			return GetFiles(parentDirectory)
				.Where(x => x.StartsWith(path.TrimEnd(extension) + "_thumb"))
				.ToList();
		}

		public void DeleteFile(string path, bool deleteThumbnails)
		{
			DeleteFile(path);

			if (!deleteThumbnails)
				return;

			DeleteThumbnails(path);
		}

		public void DeleteThumbnails(string path)
		{
			GetThumbnails(path)
				.ForEach(DeleteFile);
		}
	}
}
