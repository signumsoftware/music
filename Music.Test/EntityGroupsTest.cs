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
                Assert.AreEqual(AllLab, Database.Query<LabelDN>().Count());
                Assert.AreEqual(JapLab, Database.Query<LabelDN>().Count(r => r.InCondition(MusicGroups.JapanEntities)));

                Assert.AreEqual(AllLab, Database.RetrieveAll<LabelDN>().Count);
                Assert.AreEqual(AllLab, Database.RetrieveAllLite<LabelDN>().Count);

                Assert.AreEqual(AllLab, Database.Query<LabelDN>().WhereAllowed().Count());
            }
        }

        [TestMethod]
        public void TypeConditionQueryableAuthDisable()
        {
            using (AuthLogic.Disable())
            {
                Assert.AreEqual(AllAlb, Database.Query<AlbumDN>().Count());
                Assert.AreEqual(JapAlb, Database.Query<AlbumDN>().Count(r => r.InCondition(MusicGroups.JapanEntities)));

                Assert.AreEqual(AllAlb, Database.RetrieveAll<AlbumDN>().Count);
                Assert.AreEqual(AllAlb, Database.RetrieveAllLite<AlbumDN>().Count);

                Assert.AreEqual(AllAlb, Database.Query<AlbumDN>().WhereAllowed().Count());
            }
        }

        [TestMethod]
        public void TypeConditionExternal()
        {
            using (AuthLogic.UnsafeUserSession("external"))
            {
                Assert.AreEqual(JapLab, Database.Query<LabelDN>().Count());
                Assert.AreEqual(JapLab, Database.Query<LabelDN>().Count(r => r.InCondition(MusicGroups.JapanEntities)));

                Assert.AreEqual(JapLab, Database.RetrieveAll<LabelDN>().Count);
                Assert.AreEqual(JapLab, Database.RetrieveAllLite<LabelDN>().Count);

                using (TypeAuthLogic.DisableQueryFilter())
                {
                    Assert.AreEqual(AllLab, Database.Query<LabelDN>().Count());
                    Assert.AreEqual(AllLab, Database.RetrieveAllLite<LabelDN>().Count);
                    Assert.AreEqual(JapLab, Database.Query<LabelDN>().WhereAllowed().Count());
                }
            }
        }

        [TestMethod]
        public void TypeConditionExternalQueryable()
        {
            using (AuthLogic.UnsafeUserSession("external"))
            {
                Assert.AreEqual(JapAlb, Database.Query<AlbumDN>().Count());
                Assert.AreEqual(JapAlb, Database.Query<AlbumDN>().Count(r => r.InCondition(MusicGroups.JapanEntities)));

                Assert.AreEqual(JapAlb, Database.RetrieveAll<AlbumDN>().Count);
                Assert.AreEqual(JapAlb, Database.RetrieveAllLite<AlbumDN>().Count);

                var count = Database.Query<AlbumDN>().SelectMany(a => a.Songs).Count();
                Assert.AreEqual(count, Database.MListQuery((AlbumDN a) => a.Songs).Count()); 

                using (TypeAuthLogic.DisableQueryFilter())
                {
                    Assert.AreEqual(AllAlb, Database.Query<AlbumDN>().Count());
                    Assert.AreEqual(AllAlb, Database.RetrieveAllLite<AlbumDN>().Count);
                    Assert.AreEqual(JapAlb, Database.Query<AlbumDN>().WhereAllowed().Count());
                }
            }
        }

        [TestMethod]
        public void TypeConditionRetrieve()
        {
            using (AuthLogic.UnsafeUserSession("external"))
            {
                Assert2.Throws<EntityNotFoundException>(() => Database.Retrieve<LabelDN>(1));
                using (TypeAuthLogic.DisableQueryFilter())
                {
                    Database.Query<LabelDN>().SingleEx(r => r.Name == "Virgin");
                }
            }
        }


        [TestMethod]
        public void TypeConditionUpdate()
        {
            using (AuthLogic.UnsafeUserSession("external"))
            using (Transaction tr = new Transaction())
            {
                Assert.AreEqual(JapLab, Database.Query<LabelDN>().UnsafeUpdate().Set(r => r.Name, r=> r.Name + r.Name ).Execute());
                Assert.AreEqual(JapAlb, Database.Query<AlbumDN>().UnsafeUpdate().Set(r => r.Name, r => r.Name + r.Name).Execute());

                //tr.Commit();
            }
        }

        [TestMethod]
        public void TypeConditionDelete()
        {
            using (AuthLogic.UnsafeUserSession("external"))
            using (Transaction tr = new Transaction())
            {
                Assert.AreEqual(JapAlb, Database.Query<AlbumDN>().UnsafeDelete());
                Assert.AreEqual(JapLab, Database.Query<LabelDN>().UnsafeDelete());

                var count = Database.Query<AlbumDN>().SelectMany(a => a.Songs).Count();
                Assert.AreEqual(count, Database.MListQuery((AlbumDN a) => a.Songs).UnsafeDeleteMList()); 

                //tr.Commit();
            }
        }

        [TestMethod]
        public void TypeConditionJoin()
        {
            using (AuthLogic.UnsafeUserSession("external"))
            {
                int coutFast = Database.Query<AlbumDN>().Count();
                int coutSlow = (from lab in Database.Query<LabelDN>()
                                join alb in Database.Query<AlbumDN>() on lab equals alb.Label
                                select lab).Count();
                Assert.AreEqual(coutFast, coutSlow);
            }
        }

        [TestMethod]
        public void TypeConditionSaveOut()
        {
            using (AuthLogic.UnsafeUserSession("external"))
            using (OperationLogic.AllowSave<LabelDN>())
            using (TypeAuthLogic.DisableQueryFilter())
            {
                //Because of target
                using (Transaction tr = new Transaction())
                {
                    var label = Database.Query<LabelDN>().SingleEx(l => l.Name == "MJJ");
                    label.Owner.Retrieve().Country.Name = "Spain";
                    label.Save();

                    Assert2.Throws<UnauthorizedAccessException>(() => tr.Commit());
                }

                using (Transaction tr = new Transaction())
                {
                    var label = Database.Query<LabelDN>().SingleEx(l => l.Name == "MJJ");
                    label.Owner = Database.Query<LabelDN>().Where(l => l.Name == "Virgin").Select(a => a.ToLite()).SingleEx();
                    label.Save();

                    //tr.Commit();
                }


                //Because of origin
                using (Transaction tr = new Transaction())
                {
                    var label = Database.Query<LabelDN>().SingleEx(l => l.Name == "Virgin");
                    label.Country.Name = "Japan Empire";
                    label.Save();
                    Assert2.Throws<UnauthorizedAccessException>(() => tr.Commit());
                }

                using (Transaction tr = new Transaction())
                {
                    var label = Database.Query<LabelDN>().SingleEx(l => l.Name == "WEA International");
                    label.Owner.Retrieve().Name = "Japan Empire";
                    label.Save();
                    Assert2.Throws<UnauthorizedAccessException>(() => tr.Commit());
                }

                using (Transaction tr = new Transaction())
                {
                    var label = Database.Query<LabelDN>().SingleEx(l => l.Name == "WEA International");
                    label.Owner = Database.Query<LabelDN>().Where(l => l.Name == "Sony").Select(a => a.ToLite()).SingleEx();
                    label.Save();

                    Assert2.Throws<UnauthorizedAccessException>(() => tr.Commit());
                }
            }
        }

        [TestMethod]
        public void TypeConditionSaveTwice()
        {
            using (AuthLogic.UnsafeUserSession("external"))
            using (OperationLogic.AllowSave<LabelDN>())
            using (TypeAuthLogic.DisableQueryFilter())
            {
                var japan = Database.Query<CountryDN>().Single(l => l.Name.StartsWith("Japan"));

                var node = Database.Query<LabelDN>().OrderByDescending(a => a.Id).First().Node;

                LabelDN label;

                using (Transaction tr = new Transaction())
                {   
                    label = new LabelDN { Name = "Nintendo sound", Country = japan, Node = node.NextSibling() }.Save();

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
            using (OperationLogic.AllowSave<LabelDN>())
            using (TypeAuthLogic.DisableQueryFilter())
            {
                var japan = Database.Query<CountryDN>().Single(l => l.Name.StartsWith("Japan"));
                var node = Database.Query<LabelDN>().OrderByDescending(a => a.Id).First().Node;

                LabelDN label;

                using (Transaction tr = new Transaction())
                {
                    label = new LabelDN { Name = "Nintendo sound", Country = japan, Node = node.NextSibling() }.Save();

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
            using (OperationLogic.AllowSave<LabelDN>())
            using (TypeAuthLogic.DisableQueryFilter())
            {
                var japan = Database.Query<CountryDN>().Single(l => l.Name.StartsWith("Japan"));
                var node = Database.Query<LabelDN>().OrderByDescending(a => a.Id).First().Node;

                LabelDN label;

                using (Transaction tr = new Transaction())
                {
                    label = new LabelDN { Name = "Nintendo sound", Country = japan, Node = node.NextSibling() }.Save();

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
