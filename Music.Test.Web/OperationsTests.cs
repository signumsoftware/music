using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Signum.Web.Selenium;
using Selenium;
using System.Diagnostics;
using Signum.Engine;
using Signum.Entities.Authorization;
using Signum.Test;
using Music.Test.Web.Properties;
using Signum.Engine.Maps;
using Signum.Engine.Authorization;
using System.Text.RegularExpressions;
using Signum.Utilities;
using System.Resources;
using System.Threading;
using Signum.Test.Environment;
using Signum.Entities;
using Signum.Engine.Operations;

namespace Music.Test.Web
{
    [TestClass]
    public class OperationsTests : Common
    {
        public OperationsTests()
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
        public void Operations001_Execute_Navigate()
        {
            SearchPage(typeof(AlbumDN), CheckLogin)
                .Using(albums => albums.Create<AlbumDN>())
                .EndUsing(album =>
            {
                album.ValueLineValue(a => a.Name, "test");
                album.ValueLineValue(a => a.Year, 2010);
                album.EntityLine(a => a.Author).Find(typeof(BandDN)).SelectByPosition(0);
                album.EntityCombo(a => a.Label).SelectLabel("Virgin");
                album.ExecuteSubmit(AlbumOperation.Save);

                Assert.IsTrue(album.HasId());
                album.RuntimeInfo().ToLite().Delete();
            });
        }

        [TestMethod]
        public void Operations002_Execute_ReloadContent()
        {
            NormalPage<AlbumDN>(1, CheckLogin).EndUsing(album =>
            {
                string name = "Siamese Dreamm";
                album.ValueLineValue(a => a.Name, name);
                album.ExecuteAjax(AlbumOperation.Modify);
                selenium.Wait(() => album.Title() == name);
            });
        }

        [TestMethod]
        public void Operations003_ConstructFrom()
        {
            NormalPage<AlbumDN>(1, CheckLogin).Using(album =>
            {
                Assert.IsFalse(album.OperationEnabled(AlbumOperation.Save));

                return album.ConstructFromNormalWindow<AlbumDN>(AlbumOperation.Clone);
            }).EndUsing(album =>
            {
                album.Selenium.Wait(() => string.IsNullOrEmpty(album.ValueLineValue(a => a.Name)));
                album.ValueLineValue(a => a.Name, "test3");
                album.ValueLineValue(a => a.Year, 2010);
                album.ExecuteSubmit(AlbumOperation.Save);
                Assert.IsTrue(album.HasId());
                album.RuntimeInfo().ToLite().Delete();
            });
        }

        [TestMethod]
        public void Operations004_ConstructFrom_OpenPopup()
        {
            NormalPage<BandDN>(1, CheckLogin).Using(band =>
            {
                using (var model = band.ConstructFromPopup<AlbumFromBandModel>(AlbumOperation.CreateAlbumFromBand))
                {
                    model.ValueLineValue(m => m.Name, "test2");
                    model.ValueLineValue(m => m.Year, 2010);
                    model.EntityLine(a => a.Label).Find().SelectByPosition(0);
                    return model.OkWaitNormalPage<AlbumDN>();
                }
            }).EndUsing(album =>
            {
                Assert.IsTrue(album.HasId());

                album.RuntimeInfo().ToLite().Delete();
            });
        }

        [TestMethod]
        public void Operations005_ConstructFrom_OpenPopupAndSubmitFormAndPopup()
        {
            NormalPage<AlbumDN>(1, CheckLogin).Using(album =>
            {
                album.ButtonClick("CloneWithData");

                using (ValueLinePopup popup = new ValueLinePopup(album.Selenium))
                {
                    selenium.WaitElementPresent(popup.PopupVisibleLocator);

                    popup.StringValueLine.StringValue = "test popup";

                    return popup.OkWaitNormalPage<AlbumDN>();
                }
            })
            .EndUsing(album2 =>
            {
                Assert.IsTrue(album2.Selenium.IsTextPresent("test popup"));
            });
        }

        [TestMethod]
        public void Operations006_Delete()
        {
            Lite<AlbumDN> lite = null;
            using (AuthLogic.UnsafeUserSession("internal"))
            {
                AlbumDN album = Database.Query<AlbumDN>().First().ConstructFrom<AlbumDN>(AlbumOperation.Clone);
                album.Name = "test6";
                album.Year = 2012;
                lite = album.Execute(AlbumOperation.Save).ToLite(); 
            }

            NormalPage<AlbumDN>(lite, CheckLogin).Using(album =>
            {
                return album.DeleteSubmit(AlbumOperation.Delete);
            }).EndUsing(albums =>
            {
                albums.Search();
                selenium.AssertElementNotPresent(albums.SearchControl.Results.RowLocator(lite));
            });
        }

        [TestMethod]
        public void Operations007_ConstructFromMany()
        {
            using (var albums = SearchPage(typeof(AlbumDN), CheckLogin))
            {
                albums.Search();

                albums.SearchControl.Results.SelectRow(0, 1);

                using (var alb = albums.SearchControl.Results.SelectedClick().ConstructFromPopup<AlbumDN>(AlbumOperation.CreateEmptyGreatestHitsAlbum))
                {
                    alb.ValueLineValue(a => a.Name, "test greatest empty");
                    alb.EntityCombo(a => a.Label).SelectLabel("Virgin");
                    alb.ExecuteAjax(AlbumOperation.Save);
                    Assert.IsTrue(alb.OperationEnabled(AlbumOperation.Modify));
                    alb.RuntimeInfo().ToLite().Delete();
                }
            }
        }
    }
}
