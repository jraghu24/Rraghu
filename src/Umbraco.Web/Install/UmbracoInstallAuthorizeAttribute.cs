using System;
using System.Web;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Web.Security;
using umbraco.BasePages;

namespace Umbraco.Web.Install
{
	/// <summary>
	/// Ensures authorization occurs for the installer if it has already completed. If install has not yet occured
	/// then the authorization is successful
	/// </summary>
	internal class UmbracoInstallAuthorizeAttribute : AuthorizeAttribute
	{
		private readonly ApplicationContext _applicationContext;
        private readonly UmbracoContext _umbracoContext;

        public UmbracoInstallAuthorizeAttribute(UmbracoContext umbracoContext)
        {
            if (umbracoContext == null) throw new ArgumentNullException("umbracoContext");
            _umbracoContext = umbracoContext;
            _applicationContext = _umbracoContext.Application;
        }

		public UmbracoInstallAuthorizeAttribute()
			: this(UmbracoContext.Current)
		{
			
		}

		/// <summary>
		/// Ensures that the user must be logged in or that the application is not configured just yet.
		/// </summary>
		/// <param name="httpContext"></param>
		/// <returns></returns>
		protected override bool AuthorizeCore(HttpContextBase httpContext)
		{
		    if (httpContext == null) throw new ArgumentNullException("httpContext");

		    try
			{
				//if its not configured then we can continue
				if (!_applicationContext.IsConfigured)
				{
					return true;
				}
				
				//otherwise we need to ensure that a user is logged in
                var isLoggedIn = _umbracoContext.Security.ValidateUserContextId(_umbracoContext.Security.UmbracoUserContextId);
				if (isLoggedIn)
				{
					return true;
				}

				return false;			
			}
			catch (Exception)
			{
				return false;
			}
		}
        
        /// <summary>
        /// Override to throw exception instead of returning 401 result
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            //they aren't authorized but the app has installed
            throw new HttpException((int)global::System.Net.HttpStatusCode.Unauthorized, "You must login to view this resource.");
        }

	}
}