﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Sync;
using Umbraco.Web.Cache;
using umbraco.interfaces;

namespace Umbraco.Tests.Sync
{
    /// <summary>
    /// Ensures that calls to DistributedCache methods carry through to the IServerMessenger correctly
    /// </summary>
    [TestFixture]
    public class DistributedCacheTests
    {
        [SetUp]
        public void Setup()
        {
            ServerRegistrarResolver.Current = new ServerRegistrarResolver(
                new TestServerRegistrar());
            ServerMessengerResolver.Current = new ServerMessengerResolver(
                new TestServerMessenger());
            CacheRefreshersResolver.Current = new CacheRefreshersResolver(() => new[] { typeof(TestCacheRefresher) });
            Resolution.Freeze();
        }

        [TearDown]
        public void Teardown()
        {
            ServerRegistrarResolver.Reset();
            ServerMessengerResolver.Reset();
            CacheRefreshersResolver.Reset();
        }

        [Test]
        public void RefreshIntId()
        {
            for (var i = 0; i < 10; i++)
            {
                DistributedCache.Instance.Refresh(Guid.Parse("E0F452CB-DCB2-4E84-B5A5-4F01744C5C73"), i);
            }
            Assert.AreEqual(10, ((TestServerMessenger)ServerMessengerResolver.Current.Messenger).IntIdsRefreshed.Count);
        }

        [Test]
        public void RefreshGuidId()
        {
            for (var i = 0; i < 11; i++)
            {
                DistributedCache.Instance.Refresh(Guid.Parse("E0F452CB-DCB2-4E84-B5A5-4F01744C5C73"), Guid.NewGuid());
            }
            Assert.AreEqual(11, ((TestServerMessenger)ServerMessengerResolver.Current.Messenger).GuidIdsRefreshed.Count);
        }

        [Test]
        public void RemoveIds()
        {
            for (var i = 0; i < 12; i++)
            {
                DistributedCache.Instance.Remove(Guid.Parse("E0F452CB-DCB2-4E84-B5A5-4F01744C5C73"), i);
            }
            Assert.AreEqual(12, ((TestServerMessenger)ServerMessengerResolver.Current.Messenger).IntIdsRemoved.Count);
        }

        [Test]
        public void FullRefreshes()
        {
            for (var i = 0; i < 13; i++)
            {
                DistributedCache.Instance.RefreshAll(Guid.Parse("E0F452CB-DCB2-4E84-B5A5-4F01744C5C73"));
            }
            Assert.AreEqual(13, ((TestServerMessenger)ServerMessengerResolver.Current.Messenger).CountOfFullRefreshes);
        }

        #region internal test classes

        internal class TestCacheRefresher : ICacheRefresher
        {
            public Guid UniqueIdentifier
            {
                get { return Guid.Parse("E0F452CB-DCB2-4E84-B5A5-4F01744C5C73"); }
            }
            public string Name
            {
                get { return "Test"; }
            }
            public void RefreshAll()
            {
                
            }

            public void Refresh(int id)
            {
                
            }

            public void Remove(int id)
            {
                
            }

            public void Refresh(Guid id)
            {
               
            }
        }

        internal class TestServerMessenger : IServerMessenger
        {
            //used for tests
            public List<int> IntIdsRefreshed = new List<int>(); 
            public List<Guid> GuidIdsRefreshed = new List<Guid>();
            public List<int> IntIdsRemoved = new List<int>();
            public int CountOfFullRefreshes = 0;
            

            public void PerformRefresh<T>(IEnumerable<IServerRegistration> servers, ICacheRefresher refresher, Func<T, int> getNumericId, params T[] instances)
            {
                throw new NotImplementedException();
            }

            public void PerformRefresh<T>(IEnumerable<IServerRegistration> servers, ICacheRefresher refresher, Func<T, Guid> getGuidId, params T[] instances)
            {
                throw new NotImplementedException();
            }

            public void PerformRemove<T>(IEnumerable<IServerRegistration> servers, ICacheRefresher refresher, Func<T, int> getNumericId, params T[] instances)
            {
                throw new NotImplementedException();
            }

            public void PerformRemove(IEnumerable<IServerRegistration> servers, ICacheRefresher refresher, params int[] numericIds)
            {
                IntIdsRemoved.AddRange(numericIds);
            }

            public void PerformRefresh(IEnumerable<IServerRegistration> servers, ICacheRefresher refresher, params int[] numericIds)
            {
                IntIdsRefreshed.AddRange(numericIds);
            }

            public void PerformRefresh(IEnumerable<IServerRegistration> servers, ICacheRefresher refresher, params Guid[] guidIds)
            {
                GuidIdsRefreshed.AddRange(guidIds);
            }

            public void PerformRefreshAll(IEnumerable<IServerRegistration> servers, ICacheRefresher refresher)
            {
                CountOfFullRefreshes++;
            }
        }

        internal class TestServerRegistrar : IServerRegistrar
        {
            public IEnumerable<IServerRegistration> Registrations
            {
                get
                {
                    return new List<IServerRegistration>()
                        {
                            new TestServerRegistration("localhost")
                        };
                }
            }
        }

        public class TestServerRegistration : IServerRegistration
        {
            public TestServerRegistration(string address)
            {
                ServerAddress = address;
            }
            public string ServerAddress { get; private set; }
        }

        #endregion
    }
}