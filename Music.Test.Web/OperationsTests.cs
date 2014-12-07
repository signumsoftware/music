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
            Login();

            SearchPage(typeof(AlbumEntity))
                .Using(albums => albums.Create<AlbumEntity>())
                .EndUsing(album =>
            {
                album.ValueLineValue(a => a.Name, "test");
                album.ValueLineValue(a => a.Year, 2010);
                album.EntityLine(a => a.Author).Find(typeof(BandEntity)).SelectByPosition(0);
                album.EntityCombo(a => a.Label).SelectLabel("Virgin");
                album.ExecuteSubmit(AlbumOperation.Save);

                Assert.IsTrue(album.HasId());
                album.RuntimeInfo().ToLite().Delete();
            });
        }

        [TestMethod]
        public void Operations002_Execute_ReloadContent()
        {
            Login();
            NormalPage<AlbumEntity>(1).EndUsing(album =>
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
            Login();

            NormalPage<AlbumEntity>(1).EndUsing(album =>
            {
                Assert.IsFalse(album.OperationEnabled(AlbumOperation.Save));

                using( var newAlbum =  album.ConstructFromPopup(AlbumOperation.Clone))
                {
                    newAlbum.Selenium.Wait(() => string.IsNullOrEmpty(newAlbum.ValueLineValue(a => a.Name)));
                    newAlbum.ValueLineValue(a => a.Name, "test3");
                    newAlbum.ValueLineValue(a => a.Year, 2010);

                    newAlbum.EntityLine(a => a.BonusTrack).View<SongEntity>().EndUsing(s => Assert.IsNull(s.EntityState()));

                    newAlbum.ExecuteAjax(AlbumOperation.Save);
                    Assert.IsTrue(newAlbum.HasId());
                    newAlbum.RuntimeInfo().ToLite().Delete();
                }
            });
        }

        [TestMethod]
        public void Operations004_ConstructFrom_OpenPopup()
        {
            Login();

            NormalPage<BandEntity>(1).EndUsing(band =>
            {
                band.OperationPopup<AlbumFromBandModel>(AlbumOperation.CreateAlbumFromBand).Using(model =>
                {
                    model.ValueLineValue(m => m.Name, "test2");
                    model.ValueLineValue(m => m.Year, 2010);
                    model.EntityLine(a => a.Label).Find().SelectByPosition(0);
                    return model.OkWaitPopupControl<AlbumEntity>();
                }).EndUsing(album =>
                {
                    Assert.IsTrue(album.RuntimeInfo().IdOrNull.HasValue);

                    album.RuntimeInfo().ToLite().Delete();
                });
            }); 
        }

        [TestMethod]
        public void Operations005_ConstructFrom_OpenPopupAndSubmitFormAndPopup()
        {
            Login();

            NormalPage<AlbumEntity>(1).EndUsing(album =>
            {
                album.ButtonClick("CloneWithData");

                new ValueLinePopup(album.Selenium).Using(popup =>
                {
                    selenium.WaitElementPresent(popup.PopupLocator);

                    popup.ValueLine.StringValue = "test popup";

                    return popup.OkWaitPopupControl<AlbumEntity>();
                }).EndUsing(album2 =>
                {
                    Assert.IsTrue(album2.GetLite().ToString().Contains("test popup"));
                });
            });
        }

        [TestMethod]
        public void Operations006_Delete()
        {
            Login();

            Lite<AlbumEntity> lite = null;
            using (AuthLogic.UnsafeUserSession("internal"))
            {
                AlbumEntity album = Database.Query<AlbumEntity>().First().ConstructFrom(AlbumOperation.Clone);
                album.Name = "test6";
                album.Year = 2012;
                lite = album.Execute(AlbumOperation.Save).ToLite(); 
            }

            NormalPage<AlbumEntity>(lite).Using(album =>
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
            Login();

            using (var albums = SearchPage(typeof(AlbumEntity)))
            {
                albums.Search();

                albums.SearchControl.Results.SelectRow(0, 1);

                using (var alb = albums.SearchControl.Results.SelectedClick().MenuClickPopup<AlbumEntity>(AlbumOperation.CreateEmptyGreatestHitsAlbum))
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
