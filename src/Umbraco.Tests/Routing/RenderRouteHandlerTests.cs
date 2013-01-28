﻿using System.Web.Mvc;
using System.Web.Routing;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Tests.Stubs;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;
using Umbraco.Web.Routing;
using umbraco.BusinessLogic;

namespace Umbraco.Tests.Routing
{
	[TestFixture]
	public class RenderRouteHandlerTests : BaseRoutingTest
	{

		public override void Initialize()
		{
            //this ensures its reset
            PluginManager.Current = new PluginManager();

            SurfaceControllerResolver.Current = new SurfaceControllerResolver(
                PluginManager.Current.ResolveSurfaceControllers());

			base.Initialize();

			System.Configuration.ConfigurationManager.AppSettings.Set("umbracoPath", "~/umbraco");
            
			var webBoot = new WebBootManager(new UmbracoApplication(), true);
			//webBoot.Initialize();
			//webBoot.Startup(null); -> don't call startup, we don't want any other application event handlers to bind for this test.
			//webBoot.Complete(null);
			webBoot.CreateRoutes();
		}

		public override void TearDown()
		{
			base.TearDown();
			RouteTable.Routes.Clear();
			System.Configuration.ConfigurationManager.AppSettings.Set("umbracoPath", "");
			SurfaceControllerResolver.Reset();
		}

		/// <summary>
		/// Will route to the default controller and action since no custom controller is defined for this node route
		/// </summary>
		[Test]
		public void Umbraco_Route_Umbraco_Defined_Controller_Action()
		{
            var template = new Template("homePage");
            ApplicationContext.Current.Services.FileService.SaveTemplate(template);
			var route = RouteTable.Routes["Umbraco_default"];
			var routeData = new RouteData() { Route = route };
			var routingContext = GetRoutingContext("~/dummy-page", template.Id, routeData);
			var docRequest = new PublishedContentRequest(routingContext.UmbracoContext.CleanedUmbracoUrl, routingContext)
			{
				PublishedContent = routingContext.PublishedContentStore.GetDocumentById(routingContext.UmbracoContext, 1174),
				TemplateModel = template
			};

			var handler = new RenderRouteHandler(new TestControllerFactory(), routingContext.UmbracoContext);

			handler.GetHandlerForRoute(routingContext.UmbracoContext.HttpContext.Request.RequestContext, docRequest);
			Assert.AreEqual("RenderMvc", routeData.Values["controller"].ToString());
			Assert.AreEqual("Index", routeData.Values["action"].ToString());
		}

		//test all template name styles to match the ActionName
		[TestCase("home-page")]
		[TestCase("Home-Page")]
		[TestCase("HomePage")]
		[TestCase("homePage")]
		public void Umbraco_Route_User_Defined_Controller_Action(string templateName)
		{
            var template = new Template(templateName);
            ApplicationContext.Current.Services.FileService.SaveTemplate(template);
            var route = RouteTable.Routes["Umbraco_default"];
			var routeData = new RouteData() {Route = route};
			var routingContext = GetRoutingContext("~/dummy-page", template.Id, routeData);
			var docRequest = new PublishedContentRequest(routingContext.UmbracoContext.CleanedUmbracoUrl, routingContext)
				{
					PublishedContent = routingContext.PublishedContentStore.GetDocumentById(routingContext.UmbracoContext, 1172), 
					TemplateModel = template
				};

			var handler = new RenderRouteHandler(new TestControllerFactory(), routingContext.UmbracoContext);

			handler.GetHandlerForRoute(routingContext.UmbracoContext.HttpContext.Request.RequestContext, docRequest);
			Assert.AreEqual("CustomDocument", routeData.Values["controller"].ToString());
			Assert.AreEqual("HomePage", routeData.Values["action"].ToString());
		}


		#region Internal classes

		///// <summary>
		///// Used to test a user route (non-umbraco)
		///// </summary>
		//private class CustomUserController : Controller
		//{

		//    public ActionResult Index()
		//    {
		//        return View();
		//    }

		//    public ActionResult Test(int id)
		//    {
		//        return View();
		//    }

		//}

		/// <summary>
		/// Used to test a user route umbraco route
		/// </summary>
		public class CustomDocumentController : RenderMvcController
		{

			public ActionResult HomePage(RenderModel model)
			{
				return View();
			}

		}

		#endregion
	}
}
