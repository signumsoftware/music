using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Signum.Web.Selenium;
using System.Diagnostics;
using Signum.Engine;
using Signum.Entities.Authorization;
using Signum.Test;
using Music.Test.Web.Properties;
using Signum.Engine.Maps;
using Signum.Engine.Authorization;
using Signum.Utilities;
using Signum.Test.Environment;
using Signum.Entities;
using OpenQA.Selenium;

namespace Music.Test.Web
{
    [TestClass]
    public class LinesTests : Common
    {
        const string grammyAward = "GrammyAward";

        public LinesTests()
        {

        }

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            Common.Start();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            Common.MyTestCleanup();
        }

        [TestMethod]
        public void Lines001_EntityLine()
        {
            Login();
            using (var band = NormalPage<BandEntity>(1))
            {
                var el = band.EntityLine(b => b.LastAward);

                //view cancel 
                el.View<AmericanMusicAwardEntity>().EndUsing(la => la.Close());

                //remove
                el.Remove();
                Assert.IsFalse(el.HasEntity());

                //create with implementations
                el.CreatePopup<GrammyAwardEntity>().EndUsing(award =>
                {
                    award.ValueLineValue(a => a.Category, "test");
                    award.ExecuteAjax(AwardOperation.Save);
                    award.OkWaitClosed();
                });

                Assert.IsTrue(el.HasEntity());
                var lite = el.RuntimeInfo().ToLite();
                Assert.AreEqual(typeof(GrammyAwardEntity), el.RuntimeInfo().EntityType);
                Assert.IsFalse(el.RuntimeInfo().IsNew); //Already saved

                //find with implementations
                el.Remove();
                el.Find(typeof(GrammyAwardEntity)).SelectByPosition(0);
                Assert.IsTrue(el.HasEntity());

                el.Remove();
                el.AutoComplete(Lite.Create<GrammyAwardEntity>(1L));
                Assert.AreEqual(typeof(GrammyAwardEntity), el.RuntimeInfo().EntityType);

                lite.Delete();
            }
        }

        [TestMethod]
        public void Lines002_EntityLineInPopup()
        {
            Login();
            using (var album = NormalPage<AlbumEntity>(1))
            {
                //open popup
                using (var artist = album.EntityLine(a => a.Author).View<ArtistEntity>())
                {
                    var el = artist.EntityLine(b => b.LastAward);

                    //view cancel 
                    el.View<AmericanMusicAwardEntity>().EndUsing(la => la.Close());

                    //remove
                    el.Remove();
                    Assert.IsFalse(el.HasEntity());

                    //create with implementations
                    el.CreatePopup<AmericanMusicAwardEntity>().EndUsing(award =>
                    {
                        award.ValueLineValue(a => a.Category, "test");
                        award.ExecuteAjax(AwardOperation.Save);
                        award.OkWaitClosed();
                    });

                    var lite = el.RuntimeInfo().ToLite();


                    Assert.IsTrue(el.HasEntity());
                    Assert.AreEqual(typeof(AmericanMusicAwardEntity), el.RuntimeInfo().EntityType);
                    Assert.IsFalse(el.RuntimeInfo().IsNew); //Already saved

                    //find with implementations
                    el.Remove();
                    el.Find(typeof(AmericanMusicAwardEntity)).SelectByPosition(0);
                    Assert.IsTrue(el.HasEntity());

                    lite.Delete();
                }
            }
        }

        [TestMethod]
        public void Lines003_EntityLineDetail()
        {
            Login();
            using (var bandDetail = NormalPageUrl<BandEntity>(Url("Music/BandDetail")))
            {
                var ed = bandDetail.EntityDetail(a => a.LastAward);
                Assert.IsTrue(ed.HasEntity());

                ed.Remove();
                Assert.IsFalse(ed.HasEntity());

                //create with implementations
                ed.GetOrCreateDetailControl<AmericanMusicAwardEntity>().Do(award =>
                {
                    award.ValueLineValue(a => a.Category, "test");
                });

                ed.Remove();
                ed.Find(typeof(AmericanMusicAwardEntity)).SelectByPosition(0);
            }
        }

        [TestMethod]
        public void Lines004_EntityList()
        {
            Login();
            using (var band = NormalPage<BandEntity>(1))
            {
                var el = band.EntityList(b => b.Members);

                //Create and cancel
                el.CreatePopup<ArtistEntity>().EndUsing(artist => artist.CloseDiscardChanges());
                Assert.IsFalse(el.HasEntity(4));

                //Create and ok
                el.CreatePopup<ArtistEntity>().EndUsing(artist =>
                {
                    artist.ValueLineValue(a => a.Name, "test");
                    artist.ExecuteAjax(ArtistOperation.Save);
                    artist.OkWaitClosed();
                });

                Assert.IsTrue(el.HasEntity(4));
                var lite = el.RuntimeInfo(4).ToLite();
                Assert.AreEqual(typeof(ArtistEntity), el.RuntimeInfo(4).EntityType);
                Assert.IsFalse(el.RuntimeInfo(4).IsNew);

                //Delete
                el.Remove();
                Assert.IsFalse(el.HasEntity(4));

                //Find multiple
                el.Find().SelectByPosition(1, 2);
                Assert.IsTrue(el.HasEntity(4));
                Assert.IsTrue(el.HasEntity(5));


                var el2 = band.EntityList(a => a.OtherAwards);

                el2.CreatePopup<GrammyAwardEntity>().EndUsing(grammy =>
                {
                    grammy.ValueLineValue(a => a.Category, "test");
                    grammy.ExecuteAjax(AwardOperation.Save);
                    grammy.OkWaitClosed();
                });


                var lite2 = el2.RuntimeInfo(0).ToLite();
                Assert.IsTrue(el2.HasEntity(0));
                Assert.AreEqual(typeof(GrammyAwardEntity), el2.RuntimeInfo(0).EntityType);
                Assert.IsFalse(el2.RuntimeInfo(0).IsNew);

                //find with implementations
                el2.Find(typeof(GrammyAwardEntity)).SelectByPosition(0);
                Assert.IsTrue(el2.HasEntity(1));

                //Delete
                el2.Remove();
                Assert.IsFalse(el2.HasEntity(1));

                //View
                el2.View<GrammyAwardEntity>(0).EndUsing(grammy => grammy.Close());
                el2.View<GrammyAwardEntity>(0).EndUsing(grammy =>
                {
                    grammy.ValueLineValue(a => a.Category, "test2");
                    grammy.CloseDiscardChanges();
                });

                lite.Delete();
                lite2.Delete();
            }
        }

        [TestMethod]
        public void Lines005_EntityListInPopup()
        {
            Login();
            using (var band = NormalPage<BandEntity>(1))
            {
                //open popup
                using (var artist = band.EntityList(a => a.Members).View<ArtistEntity>(0))
                {
                    var el = artist.EntityList(a => a.Friends);

                    //create
                    el.CreatePopup<ArtistEntity>().EndUsing(friend =>
                    {
                        friend.ValueLineValue(a => a.Name, "test");
                        friend.ExecuteAjax(ArtistOperation.Save);
                        friend.OkWaitClosed();
                    });

                    Assert.IsTrue(el.HasEntity(1));
                    var lite = el.RuntimeInfo(1).ToLite();
                    Assert.AreEqual(typeof(ArtistEntity), el.RuntimeInfo(1).EntityType);
                    Assert.IsFalse(el.RuntimeInfo(1).IsNew);

                    //find multiple
                    el.Find().SelectByPosition(4, 5);
                    Assert.IsTrue(el.HasEntity(2));
                    Assert.IsTrue(el.HasEntity(3));

                    //delete multiple
                    el.Select(1);
                    el.Remove();
                    el.Select(2);
                    el.Remove();
                    Assert.IsFalse(el.HasEntity(1));
                    Assert.IsFalse(el.HasEntity(2));
                    Assert.IsTrue(el.HasEntity(3));

                    lite.Delete();
                }
            }
        }

        [TestMethod]
        public void Lines006_EntityListDetail()
        {
            Login();
            using (var band = NormalPageUrl<BandEntity>(Url("Music/BandDetail")))
            {
                var el = band.EntityListDetail(a => a.Members);
                el.DetailsDivSelector = By.CssSelector("#{0}CurrentMember".FormatWith(band.PrefixUnderscore()));

                //1st element is shown by default
                el.HasDetailEntity();

                //create
                el.CreateElement<ArtistEntity>().Do(artist =>
                {
                    artist.ValueLineValue(a => a.Name, "test");
                });

                //delete
                el.Remove();
                Assert.IsFalse(el.HasEntity(4));

                //find multiple
                el.Find().SelectByPosition(4, 5);
                Assert.IsTrue(el.HasEntity(4));
                Assert.IsTrue(el.HasEntity(5));
                Assert.IsTrue(el.HasDetailEntity());

                var el2 = band.EntityListDetail(a => a.OtherAwards);
                el2.DetailsDivSelector = By.CssSelector("#{0}CurrentAward".FormatWith(band.PrefixUnderscore()));

                el2.CreateElement<GrammyAwardEntity>().Do(grammy =>
                {
                    grammy.ValueLineValue(a => a.Category, "text");
                }); 

                //find with implementations
                el2.Find(typeof(GrammyAwardEntity)).SelectByPosition(0);
                Assert.IsTrue(el2.HasEntity(1));

                //Delete
                el2.Remove();
                el2.HasEntity(1);
            }
        }

        [TestMethod]
        public void Lines007_EntityRepeater()
        {
            Login();
            using (var band = NormalPageUrl<BandEntity>(Url("Music/BandRepeater")))
            {
                var er = band.EntityRepeater(a => a.Members);

                //All elements are shown
                Assert.IsTrue(er.HasEntity(0));
                Assert.IsTrue(er.HasEntity(1));
                Assert.IsTrue(er.HasEntity(2));
                Assert.IsTrue(er.HasEntity(3));

                //Create
                er.CreateElement<ArtistEntity>().Do(artist => artist.ValueLineValue(a => a.Name, "test"));
                Assert.IsTrue(er.HasEntity(4));

                //delete new element (created in client)
                er.Remove(4);
                Assert.IsFalse(er.HasEntity(4));

                //delete old element (created in server)
                er.Remove(0);
                Assert.IsFalse(er.HasEntity(0));

                //find multiple: it exists because Find is overriden to true in this EntityRepeater
                er.Find().SelectByPosition(3, 4);
                selenium.Wait(() => er.HasEntity(0));
                selenium.Wait(() => er.HasEntity(4));

                //move up
                By secondItemMichael = By.CssSelector("#Members_0_sfIndex[value='2']");
                By thirdItemMichael = By.CssSelector("#Members_0_sfIndex[value='3']");
                Assert.IsTrue(!selenium.IsElementPresent(secondItemMichael) && !selenium.IsElementPresent(thirdItemMichael));
                er.MoveUp(0);
                selenium.Wait(() => selenium.IsElementPresent(thirdItemMichael));
                //move down
                er.MoveDown(2); 
                selenium.Wait(() =>
                    selenium.IsElementPresent(secondItemMichael) &&
                    !selenium.IsElementPresent(thirdItemMichael));


                var er2 = band.EntityRepeater(b => b.OtherAwards); 

                //create with implementations
                er2.CreateElement<GrammyAwardEntity>().Do(g => g.ValueLineValue(a => a.Category, "test"));
                Assert.IsTrue(er2.HasEntity(0));

                //find does not exist by default
                Assert.IsFalse(selenium.IsElementPresent(er2.FindLocator));
            }
        }

        [TestMethod]
        public void Lines007_EntityStrip()
        {
            Login();
            using (var band = NormalPageUrl<BandEntity>(Url("Music/BandStrip")))
            {
                var er = band.EntityStrip(a => a.Members);

                //All elements are shown
                Assert.IsTrue(er.HasEntity(0));
                Assert.IsTrue(er.HasEntity(1));
                Assert.IsTrue(er.HasEntity(2));
                Assert.IsTrue(er.HasEntity(3));

                //Create
                er.CreatePopup<ArtistEntity>().EndUsing(artist => 
                {
                    artist.ValueLineValue(a => a.Name, "test");
                    artist.ExecuteAjax(ArtistOperation.Save);
                    artist.OkWaitClosed();
                });
                Assert.IsTrue(er.HasEntity(4));
                var lite = er.RuntimeInfo(4).ToLite();
                //delete new element (created in client)
                er.Remove(4);
                Assert.IsFalse(er.HasEntity(4));

                //delete old element (created in server)
                er.Remove(0);
                Assert.IsFalse(er.HasEntity(0));

                //find multiple: it exists because Find is overriden to true in this EntityStrip
                er.Find().SelectByPosition(4, 5);
                selenium.Wait(() => er.HasEntity(0));
                selenium.Wait(() => er.HasEntity(4));

                //move up
                By secondItemMichael = By.CssSelector("#Members_0_sfIndex[value='2']");
                By thirdItemMichael = By.CssSelector("#Members_0_sfIndex[value='3']");
                Assert.IsTrue(!selenium.IsElementPresent(secondItemMichael) && !selenium.IsElementPresent(thirdItemMichael));
                er.MoveUp(0);
                selenium.Wait(() => selenium.IsElementPresent(thirdItemMichael));
                //move down
                er.MoveDown(2);
                selenium.Wait(() =>
                    selenium.IsElementPresent(secondItemMichael) &&
                    !selenium.IsElementPresent(thirdItemMichael));


                var er2 = band.EntityStrip(b => b.OtherAwards);

                //create with implementations
                er2.CreatePopup<GrammyAwardEntity>().EndUsing(award =>
                {
                    award.ValueLineValue(a => a.Category, "test");
                    award.ExecuteAjax(AwardOperation.Save);
                    award.OkWaitClosed();
                });

                var lite2 = er2.RuntimeInfo(0).ToLite();
                Assert.IsTrue(er2.HasEntity(0));

                er2.AutoComplete(Lite.Create<GrammyAwardEntity>(2L));
                Assert.AreEqual(typeof(GrammyAwardEntity), er2.RuntimeInfo(1).EntityType);

                er2.AutoComplete(Lite.Create<AmericanMusicAwardEntity>(1L));
                Assert.AreEqual(typeof(AmericanMusicAwardEntity), er2.RuntimeInfo(2).EntityType);

                //find does not exist by default
                Assert.IsFalse(selenium.IsElementPresent(er2.FindLocator));

                lite.Delete();
                lite2.Delete();
            }
        }
    }

}
