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
using Signum.Engine.Authorization;
using Signum.Engine.Operations;
using System.Threading.Tasks;
using Microsoft.SqlServer.Types;
using Signum.Entities;
using Music.Test.Properties;
using Signum.Test.Environment;
using Signum.Entities.Reflection;

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

       

        [TestMethod]
        public void CacheInvalidationTest()
        {
            int invalidations = CacheLogic.Statistics().Single(t => t.Type == typeof(LabelEntity)).Invalidations;

            Assert.IsTrue(Schema.Current.EntityEvents<LabelEntity>().CacheController.Enabled);

            using (AuthLogic.Disable())
            using (OperationLogic.AllowSave<LabelEntity>())
            {
                using (Transaction tr = new Transaction())
                {
                    Assert.IsTrue(Schema.Current.EntityEvents<LabelEntity>().CacheController.Enabled);
                    var label = Database.Retrieve<LabelEntity>(1);

                    label.Name += " - ";

                    label.Save();

                    Assert.AreEqual(invalidations, CacheLogic.Statistics().Single(t => t.Type == typeof(LabelEntity)).Invalidations);
                    Assert.IsFalse(Schema.Current.EntityEvents<LabelEntity>().CacheController.Enabled);

                    Task.Factory.StartNew(() =>
                    {
                        Assert.IsTrue(Schema.Current.EntityEvents<LabelEntity>().CacheController.Enabled);
                    }).Wait();

                    tr.Commit();
                }

                Assert.IsTrue(invalidations < CacheLogic.Statistics().Single(t => t.Type == typeof(LabelEntity)).Invalidations);
                Assert.IsTrue(Schema.Current.EntityEvents<LabelEntity>().CacheController.Enabled);


                var l = Database.Retrieve<LabelEntity>(1);
                l.Name = "Virgin";
                l.Save();
            }
        }

        [TestMethod]
        public void CacheSealed()
        {
            using (AuthLogic.Disable())
            using (new EntityCache(EntityCacheType.ForceNewSealed))
            {
                var labels = Database.Query<LabelEntity>().ToList();
                Assert2.AssertAll(GraphExplorer.FromRoots(labels), a => a.Modified == ModifiedState.Sealed);
            }
        }

        [TestMethod]
        public void InvalidateRace()
        {
            List<LabelEntity> labels = new List<LabelEntity>();

            using (AuthLogic.Disable())
            using (OperationLogic.AllowSave<LabelEntity>())
            {
                for (int i = 0; i < 100; i++)
                {
                    LabelEntity l = new LabelEntity { Name = "Label" + DateTime.Now.Ticks, Node = SqlHierarchyId.Parse("/234234/" + i.ToString() + "/") }.Save();

                    l.ToLite().Retrieve();

                    labels.Add(l);
                }

                Database.DeleteList(labels);
            }
        }
    }
}
