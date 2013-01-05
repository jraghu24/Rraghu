﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading;
using System.Web;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Analysis;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using UmbracoExamine.DataServices;
using Examine;
using System.IO;
using System.Xml.Linq;

namespace UmbracoExamine
{

    /// <summary>
    /// An abstract provider containing the basic functionality to be able to query against
    /// Umbraco data.
    /// </summary>
    public abstract class BaseUmbracoIndexer : LuceneIndexer
    {
        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        protected BaseUmbracoIndexer()
            : base()
        {
        }

        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="indexerData"></param>
        /// <param name="indexPath"></param>
        /// <param name="dataService"></param>
        /// <param name="analyzer"></param>
        [SecuritySafeCritical]
        protected BaseUmbracoIndexer(IIndexCriteria indexerData, DirectoryInfo indexPath, IDataService dataService, Analyzer analyzer, bool async)
            : base(indexerData, indexPath, analyzer, async)
        {
            DataService = dataService;
        }

		[SecuritySafeCritical]
		protected BaseUmbracoIndexer(IIndexCriteria indexerData, Lucene.Net.Store.Directory luceneDirectory, IDataService dataService, Analyzer analyzer, bool async)
			: base(indexerData, luceneDirectory, analyzer, async)
		{
			DataService = dataService;
		}

        #endregion

        #region Properties

        /// <summary>
        /// If true, the IndexingActionHandler will be run to keep the default index up to date.
        /// </summary>
        public bool EnableDefaultEventHandler { get; protected set; }

        /// <summary>
        /// Determines if the manager will call the indexing methods when content is saved or deleted as
        /// opposed to cache being updated.
        /// </summary>
        public bool SupportUnpublishedContent { get; protected set; }

        /// <summary>
        /// The data service used for retreiving and submitting data to the cms
        /// </summary>
        public IDataService DataService { get; protected internal set; }

        /// <summary>
        /// the supported indexable types
        /// </summary>
        protected abstract IEnumerable<string> SupportedTypes { get; }

        #endregion

        #region Initialize


        /// <summary>
        /// Setup the properties for the indexer from the provider settings
        /// </summary>
        /// <param name="name"></param>
        /// <param name="config"></param>
        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            if (config["dataService"] != null && !string.IsNullOrEmpty(config["dataService"]))
            {
                //this should be a fully qualified type
                var serviceType = Type.GetType(config["dataService"]);
                DataService = (IDataService)Activator.CreateInstance(serviceType);
            }
            else if (DataService == null)
            {
                //By default, we will be using the UmbracoDataService
                //generally this would only need to be set differently for unit testing
                DataService = new UmbracoDataService();
            }

            DataService.LogService.LogLevel = LoggingLevel.Normal;

            if (config["logLevel"] != null && !string.IsNullOrEmpty(config["logLevel"]))
            {
                try
                {
                    var logLevel = (LoggingLevel)Enum.Parse(typeof(LoggingLevel), config["logLevel"]);
                    DataService.LogService.LogLevel = logLevel;
                }
                catch (ArgumentException)
                {                    
                    //FAILED
                    DataService.LogService.LogLevel = LoggingLevel.Normal;
                }
            }

            DataService.LogService.ProviderName = name;

            EnableDefaultEventHandler = true; //set to true by default
            bool enabled;
            if (bool.TryParse(config["enableDefaultEventHandler"], out enabled))
            {
                EnableDefaultEventHandler = enabled;
            }         

            DataService.LogService.AddVerboseLog(-1, string.Format("{0} indexer initializing", name));
            
            base.Initialize(name, config);
        }

        #endregion

        //public override void RebuildIndex()
        //{
        //    //we can make the indexing rebuilding operation happen asynchronously in a web context by calling an http handler.
        //    //we should only do this when async='true', the current request is running in a web context and the current user is authenticated.
        //    if (RunAsync && HttpContext.Current != null)
        //    {
        //        if (UmbracoEnsuredPage.CurrentUser != null)
        //        {
        //            RebuildIndexAsync();    
        //        }
        //        else
        //        {
        //            //don't rebuild, user is not authenticated and if async is set then we shouldn't be generating the index files non-async either
        //        }
        //    }
        //    else
        //    {
        //        base.RebuildIndex();
        //    }
        //}

        #region Protected

        /////<summary>
        ///// Calls a web request in a worker thread to rebuild the indexes
        /////</summary>
        //protected void RebuildIndexAsync()
        //{
        //    if (HttpContext.Current != null && UmbracoEnsuredPage.CurrentUser != null)
        //    {
        //        var handler = VirtualPathUtility.ToAbsolute(ExamineHandlerPath);
        //        var fullPath = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + handler + "?index=" + Name;
        //        var userContext = BasePage.umbracoUserContextID;
        //        var userContextCookie = HttpContext.Current.Request.Cookies["UserContext"];
        //        var thread = new Thread(() =>
        //        {
        //            var request = (HttpWebRequest)WebRequest.Create(fullPath);
        //            request.CookieContainer = new CookieContainer();
        //            request.CookieContainer.Add(new Cookie("UserContext", userContext, userContextCookie.Path,
        //                                                   string.IsNullOrEmpty(userContextCookie.Domain) ? "localhost" : userContextCookie.Domain));
        //            request.Timeout = Timeout.Infinite;
        //            request.UseDefaultCredentials = true;
        //            request.Method = "GET";
        //            request.Proxy = null;

        //            HttpWebResponse response;
        //            try
        //            {
        //                response = (HttpWebResponse)request.GetResponse();

        //                if (response.StatusCode != HttpStatusCode.OK)
        //                {
        //                    Log.Add(LogTypes.Custom, -1, "[UmbracoExamine] ExamineHandler request ended with an error: " + response.StatusDescription);
        //                }
        //            }
        //            catch (WebException ex)
        //            {
        //                Log.Add(LogTypes.Custom, -1, "[UmbracoExamine] ExamineHandler request threw an exception: " + ex.Message);
        //            }

        //        }) { IsBackground = true, Name = "ExamineAsyncHandler" };

        //        thread.Start();
        //    }
        //}

        /// <summary>
        /// Ensures that the node being indexed is of a correct type and is a descendent of the parent id specified.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override bool ValidateDocument(XElement node)
        {
            //check if this document is a descendent of the parent
            if (IndexerData.ParentNodeId.HasValue && IndexerData.ParentNodeId.Value > 0)
                if (!((string)node.Attribute("path")).Contains("," + IndexerData.ParentNodeId.Value.ToString() + ","))
                    return false;

            return base.ValidateDocument(node);
        }

        /// <summary>
        /// Reindexes all supported types
        /// </summary>
        protected override void PerformIndexRebuild()
        {
            foreach (var t in SupportedTypes)
            {
                IndexAll(t);
            }
        }

        public override void ReIndexNode(XElement node, string type)
        {
            if (!SupportedTypes.Contains(type))
                return;

            base.ReIndexNode(node, type);
        }

        /// <summary>
        /// Builds an xpath statement to query against Umbraco data for the index type specified, then
        /// initiates the re-indexing of the data matched.
        /// </summary>
        /// <param name="type"></param>
        protected override void PerformIndexAll(string type)
        {
            if (!SupportedTypes.Contains(type))
                return;

            var xPath = "//*[(number(@id) > 0 and (@isDoc or @nodeTypeAlias)){0}]"; //we'll add more filters to this below if needed

            var sb = new StringBuilder();

            //create the xpath statement to match node type aliases if specified
            if (IndexerData.IncludeNodeTypes.Count() > 0)
            {
                sb.Append("(");
                foreach (var field in IndexerData.IncludeNodeTypes)
                {
                    //this can be used across both schemas
                    const string nodeTypeAlias = "(@nodeTypeAlias='{0}' or (count(@nodeTypeAlias)=0 and name()='{0}'))";

                    sb.Append(string.Format(nodeTypeAlias, field));
                    sb.Append(" or ");
                }
                sb.Remove(sb.Length - 4, 4); //remove last " or "
                sb.Append(")");
            }

            //create the xpath statement to match all children of the current node.
            if (IndexerData.ParentNodeId.HasValue && IndexerData.ParentNodeId.Value > 0)
            {
                if (sb.Length > 0)
                    sb.Append(" and ");
                sb.Append("(");
                sb.Append("contains(@path, '," + IndexerData.ParentNodeId.Value + ",')"); //if the path contains comma - id - comma then the nodes must be a child
                sb.Append(")");
            }

            //create the full xpath statement to match the appropriate nodes. If there is a filter
            //then apply it, otherwise just select all nodes.
            var filter = sb.ToString();
            xPath = string.Format(xPath, filter.Length > 0 ? " and " + filter : "");

            //raise the event and set the xpath statement to the value returned
            var args = new IndexingNodesEventArgs(IndexerData, xPath, type);
            OnNodesIndexing(args);
            if (args.Cancel)
            {
                return;
            }

            xPath = args.XPath;

            DataService.LogService.AddVerboseLog(-1, string.Format("({0}) PerformIndexAll with XPATH: {1}", this.Name, xPath));

            AddNodesToIndex(xPath, type);
        }

        /// <summary>
        /// Returns an XDocument for the entire tree stored for the IndexType specified.
        /// </summary>
        /// <param name="xPath">The xpath to the node.</param>
        /// <param name="type">The type of data to request from the data service.</param>
        /// <returns>Either the Content or Media xml. If the type is not of those specified null is returned</returns>
        protected virtual XDocument GetXDocument(string xPath, string type)
        {
            if (type == IndexTypes.Content)
            {
                if (this.SupportUnpublishedContent)
                {
                    return DataService.ContentService.GetLatestContentByXPath(xPath);
                }
                else
                {
                    return DataService.ContentService.GetPublishedContentByXPath(xPath);
                }
            }
            else if (type == IndexTypes.Media)
            {
                return DataService.MediaService.GetLatestMediaByXpath(xPath);
            }
            return null;
        }
        #endregion

        #region Private
        /// <summary>
        /// Adds all nodes with the given xPath root.
        /// </summary>
        /// <param name="xPath">The x path.</param>
        /// <param name="type">The type.</param>
        private void AddNodesToIndex(string xPath, string type)
        {
            // Get all the nodes of nodeTypeAlias == nodeTypeAlias
            XDocument xDoc = GetXDocument(xPath, type);
            if (xDoc != null)
            {
                XElement rootNode = xDoc.Root;

                IEnumerable<XElement> children = rootNode.Elements();

                AddNodesToIndex(children, type);
            }

        }
        #endregion
    }
}
