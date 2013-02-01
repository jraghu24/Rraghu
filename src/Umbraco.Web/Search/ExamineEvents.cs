﻿using System;
using System.Linq;
using System.Security;
using System.Xml;
using System.Xml.Linq;
using Examine;
using Examine.LuceneEngine;
using Lucene.Net.Documents;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using UmbracoExamine;
using umbraco;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.member;
using umbraco.interfaces;
using Content = umbraco.cms.businesslogic.Content;
using Document = umbraco.cms.businesslogic.web.Document;

namespace Umbraco.Web.Search
{
	/// <summary>
	/// Used to wire up events for Examine
	/// </summary>
	public class ExamineEvents : IApplicationEventHandler
	{
		public void OnApplicationInitialized(UmbracoApplicationBase httpApplication, ApplicationContext applicationContext)
		{			
		}

		public void OnApplicationStarting(UmbracoApplicationBase httpApplication, ApplicationContext applicationContext)
		{			
		}

		/// <summary>
		/// Once the application has started we should bind to all events and initialize the providers.
		/// </summary>
		/// <param name="httpApplication"></param>
		/// <param name="applicationContext"></param>
		/// <remarks>
		/// We need to do this on the Started event as to guarantee that all resolvers are setup properly.
		/// </remarks>
		[SecuritySafeCritical]
		public void OnApplicationStarted(UmbracoApplicationBase httpApplication, ApplicationContext applicationContext)
		{
            //do not initialize if the application is not configured
            //We need to check if we actually can initialize, if not then don't continue
            if (ApplicationContext.Current == null
                || !ApplicationContext.Current.IsConfigured
                || !ApplicationContext.Current.DatabaseContext.IsDatabaseConfigured)
            {
                LogHelper.Info<ExamineEvents>("Not initializing Examine because the application and/or database is not configured");
                return;
            }

            LogHelper.Info<ExamineEvents>("Initializing Examine and binding to business logic events");

			var registeredProviders = ExamineManager.Instance.IndexProviderCollection
				.OfType<BaseUmbracoIndexer>().Count(x => x.EnableDefaultEventHandler);

			LogHelper.Info<ExamineEvents>("Adding examine event handlers for index providers: {0}", () => registeredProviders);

			//don't bind event handlers if we're not suppose to listen
			if (registeredProviders == 0)
				return;

            MediaService.Saved += MediaServiceSaved;
            MediaService.Deleted += MediaServiceDeleted;
            MediaService.Moved += MediaServiceMoved;
            ContentService.Saved += ContentServiceSaved;
            ContentService.Deleted += ContentService_Deleted;
            ContentService.Moved += ContentService_Moved;

			//These should only fire for providers that DONT have SupportUnpublishedContent set to true
			content.AfterUpdateDocumentCache += ContentAfterUpdateDocumentCache;
			content.AfterClearDocumentCache += ContentAfterClearDocumentCache;

			Member.AfterSave += MemberAfterSave;
			Member.AfterDelete += MemberAfterDelete;

			var contentIndexer = ExamineManager.Instance.IndexProviderCollection["InternalIndexer"] as UmbracoContentIndexer;
			if (contentIndexer != null)
			{
				contentIndexer.DocumentWriting += IndexerDocumentWriting;
			}
			var memberIndexer = ExamineManager.Instance.IndexProviderCollection["InternalMemberIndexer"] as UmbracoMemberIndexer;
			if (memberIndexer != null)
			{
				memberIndexer.DocumentWriting += IndexerDocumentWriting;
			}
		}

        [SecuritySafeCritical]
        void ContentService_Moved(IContentService sender, Umbraco.Core.Events.MoveEventArgs<IContent> e)
        {
            IndexConent(e.Entity);
        }

        [SecuritySafeCritical]
        void ContentService_Deleted(IContentService sender, Umbraco.Core.Events.DeleteEventArgs<IContent> e)
        {
            e.DeletedEntities.ForEach(
                content =>
                ExamineManager.Instance.DeleteFromIndex(
                    content.Id.ToString(),
                    ExamineManager.Instance.IndexProviderCollection.OfType<BaseUmbracoIndexer>().Where(x => x.EnableDefaultEventHandler)));
        }

        [SecuritySafeCritical]
        void ContentServiceSaved(IContentService sender, Umbraco.Core.Events.SaveEventArgs<IContent> e)
        {
            e.SavedEntities.ForEach(IndexConent);
        }

        [SecuritySafeCritical]
        void MediaServiceMoved(IMediaService sender, Umbraco.Core.Events.MoveEventArgs<IMedia> e)
        {
            IndexMedia(e.Entity);
        }

        [SecuritySafeCritical]
        void MediaServiceDeleted(IMediaService sender, Umbraco.Core.Events.DeleteEventArgs<IMedia> e)
        {
            e.DeletedEntities.ForEach(
                media =>
                ExamineManager.Instance.DeleteFromIndex(
                    media.Id.ToString(),
                    ExamineManager.Instance.IndexProviderCollection.OfType<BaseUmbracoIndexer>().Where(x => x.EnableDefaultEventHandler)));
        }

        [SecuritySafeCritical]
        void MediaServiceSaved(IMediaService sender, Umbraco.Core.Events.SaveEventArgs<IMedia> e)
        {
            e.SavedEntities.ForEach(IndexMedia);
        }

		[SecuritySafeCritical]
		private static void MemberAfterSave(Member sender, SaveEventArgs e)
		{
			//ensure that only the providers are flagged to listen execute
			var xml = sender.ToXml(new System.Xml.XmlDocument(), false).ToXElement();
			var providers = ExamineManager.Instance.IndexProviderCollection.OfType<BaseUmbracoIndexer>()
				.Where(x => x.EnableDefaultEventHandler);
			ExamineManager.Instance.ReIndexNode(xml, IndexTypes.Member, providers);
		}

		[SecuritySafeCritical]
		private static void MemberAfterDelete(Member sender, DeleteEventArgs e)
		{
			var nodeId = sender.Id.ToString();

			//ensure that only the providers are flagged to listen execute
			ExamineManager.Instance.DeleteFromIndex(nodeId,
				ExamineManager.Instance.IndexProviderCollection.OfType<BaseUmbracoIndexer>()
					.Where(x => x.EnableDefaultEventHandler));
		}

		/// <summary>
		/// Only Update indexes for providers that dont SupportUnpublishedContent
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		[SecuritySafeCritical]
		private static void ContentAfterUpdateDocumentCache(Document sender, DocumentCacheEventArgs e)
		{
			//ensure that only the providers that have DONT unpublishing support enabled       
			//that are also flagged to listen
			ExamineManager.Instance.ReIndexNode(ToXDocument(sender, true).Root, IndexTypes.Content,
				ExamineManager.Instance.IndexProviderCollection.OfType<BaseUmbracoIndexer>()
					.Where(x => !x.SupportUnpublishedContent
						&& x.EnableDefaultEventHandler));
		}

		/// <summary>
		/// Only update indexes for providers that don't SupportUnpublishedContnet
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		[SecuritySafeCritical]
		private static void ContentAfterClearDocumentCache(Document sender, DocumentCacheEventArgs e)
		{
			var nodeId = sender.Id.ToString();
			//ensure that only the providers that DONT have unpublishing support enabled           
			//that are also flagged to listen
			ExamineManager.Instance.DeleteFromIndex(nodeId,
				ExamineManager.Instance.IndexProviderCollection.OfType<BaseUmbracoIndexer>()
					.Where(x => !x.SupportUnpublishedContent
						&& x.EnableDefaultEventHandler));
		}

		/// <summary>
		/// Event handler to create a lower cased version of the node name, this is so we can support case-insensitive searching and still
		/// use the Whitespace Analyzer
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		[SecuritySafeCritical]
		private static void IndexerDocumentWriting(object sender, DocumentWritingEventArgs e)
		{
			if (e.Fields.Keys.Contains("nodeName"))
			{
				//add the lower cased version
				e.Document.Add(new Field("__nodeName",
										e.Fields["nodeName"].ToLower(),
										Field.Store.YES,
										Field.Index.ANALYZED,
										Field.TermVector.NO
										));
			}
		}


        private void IndexMedia(IMedia sender)
        {
            ExamineManager.Instance.ReIndexNode(
                sender.ToXml(), "media",
                ExamineManager.Instance.IndexProviderCollection.OfType<BaseUmbracoIndexer>().Where(x => x.EnableDefaultEventHandler));
        }

        private void IndexConent(IContent sender)
        {
            ExamineManager.Instance.ReIndexNode(
                sender.ToXml(), "content",
                ExamineManager.Instance.IndexProviderCollection.OfType<BaseUmbracoIndexer>().Where(x => x.EnableDefaultEventHandler));
        }

		/// <summary>
		/// Converts a content node to XDocument
		/// </summary>
		/// <param name="node"></param>
		/// <param name="cacheOnly">true if data is going to be returned from cache</param>
		/// <returns></returns>
		/// <remarks>
		/// If the type of node is not a Document, the cacheOnly has no effect, it will use the API to return
		/// the xml. 
		/// </remarks>
		[SecuritySafeCritical]
		public static XDocument ToXDocument(Content node, bool cacheOnly)
		{
			if (cacheOnly && node.GetType().Equals(typeof(Document)))
			{
				var umbXml = library.GetXmlNodeById(node.Id.ToString());
				if (umbXml != null)
				{
					return umbXml.ToXDocument();
				}
			}

			//this will also occur if umbraco hasn't cached content yet....

			//if it's not a using cache and it's not cacheOnly, then retrieve the Xml using the API
			return ToXDocument(node);
		}

		/// <summary>
		/// Converts a content node to Xml
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		[SecuritySafeCritical]
		private static XDocument ToXDocument(Content node)
		{
			var xDoc = new XmlDocument();
			var xNode = xDoc.CreateNode(XmlNodeType.Element, "node", "");
			node.XmlPopulate(xDoc, ref xNode, false);

			if (xNode.Attributes["nodeTypeAlias"] == null)
			{
				//we'll add the nodeTypeAlias ourselves                                
				XmlAttribute d = xDoc.CreateAttribute("nodeTypeAlias");
				d.Value = node.ContentType.Alias;
				xNode.Attributes.Append(d);
			}

			return new XDocument(xNode.ToXElement());
		}

	}
}