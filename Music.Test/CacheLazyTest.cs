﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Signum.Engine.Maps;
using Signum.Engine;
using System.Data.SqlClient;
using Signum.Utilities;
using System.Threading;
using Signum.Test;
using Signum.Engine.Cache;
using Music.Extensions.Properties;
using Signum.Engine.Authorization;
using Signum.Engine.Operations;
using System.Threading.Tasks;

namespace Music.Test
{
    [TestClass]
    public class GlobalQueryLazyTest
    {
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            Starter.StartAndLoad(UserConnections.Replace(Settings.Default.ConnectionString));
        }

        static ResetLazy<List<string>> albumNames = CacheLazy.Create(() => 
            Database.Query<AlbumDN>().Select(a => a.Name).ToList());

        static ResetLazy<List<Tuple<string, string>>> albumLabelNames = CacheLazy.Create(() =>
            (from a in Database.Query<AlbumDN>().Where(a => a.Id > 1)
             join l in Database.Query<LabelDN>() on a.Label equals l
             select Tuple.Create(a.Name, l.Name)).ToList());

        [TestMethod]
        public void NoMetadata()
        {
            using (AuthLogic.Disable())
            {

                {
                    var query = Database.Query<AlbumDN>().Where(a => a.Id == 1);

                    LoadAll();

                    query.UnsafeUpdate(a => new AlbumDN { Name = a.Name + "hola" });

                    AssertInvalidated(albumes: true, albumLabels: false);

                    LoadAll();

                    query.UnsafeUpdate(a => new AlbumDN { Name = a.Name.Substring(0, a.Name.Length - 4) }); //rollback

                    AssertInvalidated(albumes: true, albumLabels: false);
                }

                {
                    var query = Database.Query<AlbumDN>().Where(a => a.Id == 3);

                    LoadAll();

                    query.UnsafeUpdate(a => new AlbumDN { Name = a.Name + "hola" });

                    AssertInvalidated(albumes: true, albumLabels: true);

                    LoadAll();

                    query.UnsafeUpdate(a => new AlbumDN { Name = a.Name.Substring(0, a.Name.Length - 4) }); //rollback

                    AssertInvalidated(albumes: true, albumLabels: true);
                }

                //Does not invalidate
                {
                    var query = Database.Query<LabelDN>().Where(a => a.Id == 1);

                    LoadAll();

                    query.UnsafeUpdate(a => new LabelDN { Name = a.Name + "hola" });

                    AssertInvalidated(albumes: false, albumLabels: true);

                    LoadAll();

                    query.UnsafeUpdate(a => new LabelDN { Name = a.Name.Substring(0, a.Name.Length - 4) }); //rollback

                    AssertInvalidated(albumes: false, albumLabels: true);
                }

                //Does not invalidate
                {
                    LoadAll();

                    var label = new LabelDN
                    {
                        Name = "NewLabel"
                    }.Save();

                    AssertInvalidated(albumes: false, albumLabels: false);

                    LoadAll();

                    label.Delete();

                    AssertInvalidated(albumes: false, albumLabels: false);
                }
            }
        }

        static void LoadAll()
        {
            albumNames.Load();
            albumLabelNames.Load();
        }

        static void AssertInvalidated(bool albumes, bool albumLabels)
        {
            Thread.Sleep(10);

            Assert.AreEqual(albumNames.IsValueCreated, !albumes);
            Assert.AreEqual(albumLabelNames.IsValueCreated, !albumLabels);
        }

        [TestMethod]
        public void CacheInvalidationTest()
        {
            int invalidations = CacheLogic.Statistics().Single(t => t.Type == typeof(LabelDN)).Invalidations;

            Assert.IsTrue(Schema.Current.EntityEvents<LabelDN>().CacheController.Enabled);

            using (AuthLogic.Disable())
            using (OperationLogic.AllowSave<LabelDN>())
            using (Transaction tr = new Transaction())
            {
                Assert.IsTrue(Schema.Current.EntityEvents<LabelDN>().CacheController.Enabled);
                var label = Database.Retrieve<LabelDN>(1);

                label.Name += " - ";

                label.Save();

                Assert.AreEqual(invalidations, CacheLogic.Statistics().Single(t => t.Type == typeof(LabelDN)).Invalidations);
                Assert.IsFalse(Schema.Current.EntityEvents<LabelDN>().CacheController.Enabled);

                Task.Factory.StartNew(() =>
                {
                    Assert.IsTrue(Schema.Current.EntityEvents<LabelDN>().CacheController.Enabled);
                }).Wait();

                tr.Commit();
            }

            Assert.AreEqual(invalidations + 1, CacheLogic.Statistics().Single(t => t.Type == typeof(LabelDN)).Invalidations);
            Assert.IsTrue(Schema.Current.EntityEvents<LabelDN>().CacheController.Enabled);
        }
    }
}
