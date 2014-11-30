using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Signum.Entities.Authorization;
using Signum.Entities;
using Signum.Utilities.Reflection;
using Signum.Engine.Maps;
using Signum.Engine.Authorization;
using Signum.Engine.DynamicQuery;
using Signum.Engine;
using Signum.Services;
using Signum.Engine.Basics;
using Signum.Utilities;
using System.Xml.Linq;
using Signum.Engine.Exceptions;
using Signum.Engine.Operations;
using Signum.Test;
using Music.Test.Properties;
using Signum.Test.Environment;

namespace Music.Test
{
    [TestClass]
    public class TypeConditionTest
    {

        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            Starter.StartAndLoad(UserConnections.Replace(Settings.Default.ConnectionString));
        }


        [TestInitialize]
        public void Initialize()
        {
            Connector.CurrentLogger = new DebugTextWriter();
        }

        const int JapLab = 2;
        const int AllLab = 7;

        const int JapAlb = 5;
        const int AllAlb = 12; 


        [TestMethod]
        public void TypeConditionAuthDisable()
        {
            using (AuthLogic.Disable())
            {
                Assert.AreEqual(AllLab, Database.Query<LabelEntity>().Count());
                Assert.AreEqual(JapLab, Database.Query<LabelEntity>().Count(r => r.InCondition(MusicGroups.JapanEntities)));

                Assert.AreEqual(AllLab, Database.RetrieveAll<LabelEntity>().Count);
                Assert.AreEqual(AllLab, Database.RetrieveAllLite<LabelEntity>().Count);

                Assert.AreEqual(AllLab, Database.Query<LabelEntity>().WhereAllowed().Count());
            }
        }

        [TestMethod]
        public void TypeConditionQueryableAuthDisable()
        {
            using (AuthLogic.Disable())
            {
                Assert.AreEqual(AllAlb, Database.Query<AlbumEntity>().Count());
                Assert.AreEqual(JapAlb, Database.Query<AlbumEntity>().Count(r => r.InCondition(MusicGroups.JapanEntities)));

                Assert.AreEqual(AllAlb, Database.RetrieveAll<AlbumEntity>().Count);
                Assert.AreEqual(AllAlb, Database.RetrieveAllLite<AlbumEntity>().Count);

                Assert.AreEqual(AllAlb, Database.Query<AlbumEntity>().WhereAllowed().Count());
            }
        }

        [TestMethod]
        public void TypeConditionExternal()
        {
            using (AuthLogic.UnsafeUserSession("external"))
            {
                Assert.AreEqual(JapLab, Database.Query<LabelEntity>().Count());
                Assert.AreEqual(JapLab, Database.Query<LabelEntity>().Count(r => r.InCondition(MusicGroups.JapanEntities)));

                Assert.AreEqual(JapLab, Database.RetrieveAll<LabelEntity>().Count);
                Assert.AreEqual(JapLab, Database.RetrieveAllLite<LabelEntity>().Count);

                using (TypeAuthLogic.DisableQueryFilter())
                {
                    Assert.AreEqual(AllLab, Database.Query<LabelEntity>().Count());
                    Assert.AreEqual(AllLab, Database.RetrieveAllLite<LabelEntity>().Count);
                    Assert.AreEqual(JapLab, Database.Query<LabelEntity>().WhereAllowed().Count());
                }
            }
        }

        [TestMethod]
        public void TypeConditionExternalQueryable()
        {
            using (AuthLogic.UnsafeUserSession("external"))
            {
                Assert.AreEqual(JapAlb, Database.Query<AlbumEntity>().Count());
                Assert.AreEqual(JapAlb, Database.Query<AlbumEntity>().Count(r => r.InCondition(MusicGroups.JapanEntities)));

                Assert.AreEqual(JapAlb, Database.RetrieveAll<AlbumEntity>().Count);
                Assert.AreEqual(JapAlb, Database.RetrieveAllLite<AlbumEntity>().Count);

                var count = Database.Query<AlbumEntity>().SelectMany(a => a.Songs).Count();
                Assert.AreEqual(count, Database.MListQuery((AlbumEntity a) => a.Songs).Count()); 

                using (TypeAuthLogic.DisableQueryFilter())
                {
                    Assert.AreEqual(AllAlb, Database.Query<AlbumEntity>().Count());
                    Assert.AreEqual(AllAlb, Database.RetrieveAllLite<AlbumEntity>().Count);
                    Assert.AreEqual(JapAlb, Database.Query<AlbumEntity>().WhereAllowed().Count());
                }
            }
        }

        [TestMethod]
        public void TypeConditionRetrieve()
        {
            using (AuthLogic.UnsafeUserSession("external"))
            {
                Assert2.Throws<EntityNotFoundException>(() => Database.Retrieve<LabelEntity>(1));
                using (TypeAuthLogic.DisableQueryFilter())
                {
                    Database.Query<LabelEntity>().SingleEx(r => r.Name == "Virgin");
                }
            }
        }


        [TestMethod]
        public void TypeConditionUpdate()
        {
            using (AuthLogic.UnsafeUserSession("external"))
            using (Transaction tr = new Transaction())
            {
                Assert.AreEqual(JapLab, Database.Query<LabelEntity>().UnsafeUpdate().Set(r => r.Name, r=> r.Name + r.Name ).Execute());
                Assert.AreEqual(JapAlb, Database.Query<AlbumEntity>().UnsafeUpdate().Set(r => r.Name, r => r.Name + r.Name).Execute());

                //tr.Commit();
            }
        }

        [TestMethod]
        public void TypeConditionDelete()
        {
            using (AuthLogic.UnsafeUserSession("external"))
            using (Transaction tr = new Transaction())
            {
                Assert.AreEqual(JapAlb, Database.Query<AlbumEntity>().UnsafeDelete());
                Assert.AreEqual(JapLab, Database.Query<LabelEntity>().UnsafeDelete());

                var count = Database.Query<AlbumEntity>().SelectMany(a => a.Songs).Count();
                Assert.AreEqual(count, Database.MListQuery((AlbumEntity a) => a.Songs).UnsafeDeleteMList()); 

                //tr.Commit();
            }
        }

        [TestMethod]
        public void TypeConditionJoin()
        {
            using (AuthLogic.UnsafeUserSession("external"))
            {
                int coutFast = Database.Query<AlbumEntity>().Count();
                int coutSlow = (from lab in Database.Query<LabelEntity>()
                                join alb in Database.Query<AlbumEntity>() on lab equals alb.Label
                                select lab).Count();
                Assert.AreEqual(coutFast, coutSlow);
            }
        }

        [TestMethod]
        public void TypeConditionSaveOut()
        {
            using (AuthLogic.UnsafeUserSession("external"))
            using (OperationLogic.AllowSave<LabelEntity>())
            using (TypeAuthLogic.DisableQueryFilter())
            {
                //Because of target
                using (Transaction tr = new Transaction())
                {
                    var label = Database.Query<LabelEntity>().SingleEx(l => l.Name == "MJJ");
                    label.Owner.Retrieve().Country.Name = "Spain";
                    label.Save();

                    Assert2.Throws<UnauthorizedAccessException>(() => tr.Commit());
                }

                using (Transaction tr = new Transaction())
                {
                    var label = Database.Query<LabelEntity>().SingleEx(l => l.Name == "MJJ");
                    label.Owner = Database.Query<LabelEntity>().Where(l => l.Name == "Virgin").Select(a => a.ToLite()).SingleEx();
                    label.Save();

                    //tr.Commit();
                }


                //Because of origin
                using (Transaction tr = new Transaction())
                {
                    var label = Database.Query<LabelEntity>().SingleEx(l => l.Name == "Virgin");
                    label.Country.Name = "Japan Empire";
                    label.Save();
                    Assert2.Throws<UnauthorizedAccessException>(() => tr.Commit());
                }

                using (Transaction tr = new Transaction())
                {
                    var label = Database.Query<LabelEntity>().SingleEx(l => l.Name == "WEA International");
                    label.Owner.Retrieve().Name = "Japan Empire";
                    label.Save();
                    Assert2.Throws<UnauthorizedAccessException>(() => tr.Commit());
                }

                using (Transaction tr = new Transaction())
                {
                    var label = Database.Query<LabelEntity>().SingleEx(l => l.Name == "WEA International");
                    label.Owner = Database.Query<LabelEntity>().Where(l => l.Name == "Sony").Select(a => a.ToLite()).SingleEx();
                    label.Save();

                    Assert2.Throws<UnauthorizedAccessException>(() => tr.Commit());
                }
            }
        }

        [TestMethod]
        public void TypeConditionSaveTwice()
        {
            using (AuthLogic.UnsafeUserSession("external"))
            using (OperationLogic.AllowSave<LabelEntity>())
            using (TypeAuthLogic.DisableQueryFilter())
            {
                var japan = Database.Query<CountryEntity>().Single(l => l.Name.StartsWith("Japan"));

                var node = Database.Query<LabelEntity>().OrderByDescending(a => a.Id).First().Node;

                LabelEntity label;

                using (Transaction tr = new Transaction())
                {   
                    label = new LabelEntity { Name = "Nintendo sound", Country = japan, Node = node.NextSibling() }.Save();

                    label.Name = "Nintendo Sound Systems";
                    label.Save();

                    tr.Commit();
                }

                Database.Delete(label);
            }
        }

        [TestMethod]
        public void TypeConditionSaveTwiceNamedRollback()
        {
            using (AuthLogic.UnsafeUserSession("external"))
            using (OperationLogic.AllowSave<LabelEntity>())
            using (TypeAuthLogic.DisableQueryFilter())
            {
                var japan = Database.Query<CountryEntity>().Single(l => l.Name.StartsWith("Japan"));
                var node = Database.Query<LabelEntity>().OrderByDescending(a => a.Id).First().Node;

                LabelEntity label;

                using (Transaction tr = new Transaction())
                {
                    label = new LabelEntity { Name = "Nintendo sound", Country = japan, Node = node.NextSibling() }.Save();

                    using (Transaction tr2 = Transaction.NamedSavePoint("bla"))
                    {
                        label.Name = "Nintendo Sound Systems";
                        label.Save();
                        //tr2.Commit();
                    }

                    tr.Commit();
                }

                label = label.ToLite().Retrieve();

                Assert.AreEqual("Nintendo sound", label.Name);

                Database.Delete(label);
            }
        }


        [TestMethod]
        public void TypeConditionSaveTwiceNamed()
        {
            using (AuthLogic.UnsafeUserSession("external"))
            using (OperationLogic.AllowSave<LabelEntity>())
            using (TypeAuthLogic.DisableQueryFilter())
            {
                var japan = Database.Query<CountryEntity>().Single(l => l.Name.StartsWith("Japan"));
                var node = Database.Query<LabelEntity>().OrderByDescending(a => a.Id).First().Node;

                LabelEntity label;

                using (Transaction tr = new Transaction())
                {
                    label = new LabelEntity { Name = "Nintendo sound", Country = japan, Node = node.NextSibling() }.Save();

                    using (Transaction tr2 = Transaction.NamedSavePoint("bla"))
                    {
                        label.Name = "Nintendo Sound Systems";
                        label.Save();
                        tr2.Commit();
                    }

                    tr.Commit();
                }

                label = label.ToLite().Retrieve();

                Assert.AreEqual("Nintendo Sound Systems", label.Name);

                Database.Delete(label);
            }
        }

        //[TestMethod]
        //public void ImportAuthRules()
        //{
        //    AuthLogic.GloballyEnabled = false;
        //    var rules = AuthLogic.ImportRulesScript(XDocument.Load(@"C:\Users\olmo.SIGNUMS\Desktop\AuthRules.xml")); 
        //}

    }
}
