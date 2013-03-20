﻿using System;
using Umbraco.Core;
using Umbraco.Core.Cache;
using umbraco.cms.businesslogic.member;
using umbraco.interfaces;

namespace Umbraco.Web.Cache
{
    /// <summary>
    /// A cache refresher to ensure member cache is updated when members change
    /// </summary>
    /// <remarks>
    /// This is not intended to be used directly in your code and it should be sealed but due to legacy code we cannot seal it.
    /// </remarks>
    public class MemberCacheRefresher : ICacheRefresher<Member>
    {

        public Guid UniqueIdentifier
        {
            get { return new Guid(DistributedCache.MemberCacheRefresherId); }
        }

        public string Name
        {
            get { return "Clears Member Cache from umbraco.library"; }
        }

        public void RefreshAll()
        {
        }

        public void Refresh(int id)
        {
            ClearCache(id);
        }

        public void Remove(int id)
        {
            ClearCache(id);
        }

        public void Refresh(Guid id)
        {
            
        }

        private void ClearCache(int id)
        {
            ApplicationContext.Current.ApplicationCache.
                ClearCacheByKeySearch(string.Format("{0}_{1}", CacheKeys.MemberCacheKey, id));
        }

        public void Refresh(Member instance)
        {
            throw new NotImplementedException();
        }

        public void Remove(Member instance)
        {
            throw new NotImplementedException();
        }
    }
}