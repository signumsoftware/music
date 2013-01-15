using System;
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
using Microsoft.SqlServer.Types;
using Signum.Entities;

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

        static ResetLazy<List<string>> albumNames = GlobalLazy.Create(() =>
            Database.Query<AlbumDN>().Select(a => a.Name).ToList(),
            new InvalidateWith(typeof(AlbumDN)));

        static ResetLazy<List<Tuple<string, string>>> albumLabelNames = GlobalLazy.Create(() =>
            (from a in Database.Query<AlbumDN>().Where(a => a.Id > 1)
             join l in Database.Query<LabelDN>() on a.Label equals l
             select Tuple.Create(a.Name, l.Name)).ToList(),
             new InvalidateWith(typeof(LabelDN)));

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

        [TestMethod]
        public void InvalidateRace()
        {
            using (AuthLogic.Disable())
            using (OperationLogic.AllowSave<LabelDN>())
            for (int i = 0; i < 100; i++)
            {
                LabelDN l = new LabelDN { Name = "Label" + DateTime.Now.Ticks, Node = SqlHierarchyId.Parse("/234234/" + i.ToString() + "/") }.Save();

                l.ToLite().Retrieve();
            }
        }
    }
}
