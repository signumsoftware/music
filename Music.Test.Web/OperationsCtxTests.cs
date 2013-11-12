using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Signum.Web.Selenium;
using System.Text.RegularExpressions;
using Signum.Utilities;
using Signum.Entities.Processes;
using Signum.Engine.Authorization;
using Signum.Engine;
using Signum.Test;
using Signum.Test.Environment;
using Signum.Engine.Processes;
using System.IO;

namespace Music.Test.Web
{
    [TestClass]
    public class OperationsCtxTests : Common
    {
        public OperationsCtxTests()
        {

        }

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            Common.Start();

            ProcessRunnerLogic.StartRunningProcesses(1000);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            Common.MyTestCleanup();
        }

        [TestMethod]
        public void OperationCtx001_Execute()
        {
            using (var artists = SearchPage(typeof(ArtistDN), CheckLogin))
            {
                artists.Results.OrderBy("Id");

                selenium.Wait(() => selenium.IsElementPresent(SearchTestExtensions.RowSelector(selenium, 1)));

                artists.SearchControl.Results.EntityContextMenu(0).ExecuteClick(ArtistOperation.AssignPersonalAward);

                Assert.IsTrue(artists.SearchControl.Results.EntityContextMenu(0).IsDisabled(ArtistOperation.AssignPersonalAward)); 
            }
        }

        [TestMethod]
        public void OperationCtx002_ConstructFrom_OpenPopup()
        {
            SearchPage(typeof(BandDN), CheckLogin).Using(bands =>
            {
                bands.SearchControl.Search();

                return bands.SearchControl.Results.EntityContextMenu(0).ConstructFromPopup<AlbumDN>(AlbumOperation.CreateAlbumFromBand).Using(album =>
                {
                    album.ValueLineValue(a => a.Name, "ctxtest");
                    album.ValueLineValue(a => a.Year, DateTime.Now.Year);
                    album.EntityLine(a => a.Label).Find().SelectByPosition(0);
                    return album.OkWaitNormalPage<AlbumDN>();
                });
            }).EndUsing(album =>
            {
                Assert.IsTrue(album.HasId());
            }); 
        }

        [TestMethod]
        public void OperationCtx003_Delete()
        {
            using (var albums = SearchPage(typeof(AlbumDN), CheckLogin))
            {
                int count = AuthLogic.Disable().Using(d => Database.Query<AlbumDN>().Count());

                albums.Results.OrderByDescending("Id");

                Assert.AreEqual(count, albums.Results.RowsCount());

                albums.Results.EntityContextMenu(0).ExecuteClick(AlbumOperation.Delete);

                albums.Selenium.ConsumeConfirmation();

                albums.Search();
                albums.Search();

                Assert.AreEqual(count - 1, albums.Results.RowsCount());
            }
        }

        [TestMethod]
        public void OperationCtx004_ConstructFromMany()
        {
            SearchPage(typeof(AlbumDN), CheckLogin).Using(albums =>
            {
                albums.Search();
                albums.Results.SelectRow(0, 1);

                return albums.Results.EntityContextMenu(1).ConstructFromNormalPage<AlbumDN>(AlbumOperation.CreateGreatestHitsAlbum);
            }).EndUsing(album =>
            {
                album.ValueLineValue(a => a.Name, "test greatest hits");
                album.EntityCombo(a => a.Label).SelectLabel("Virgin");
                album.ExecuteSubmit(AlbumOperation.Save);
                Assert.IsTrue(album.HasId());
            });
        }

        [TestMethod]
        public void OperationCtx010_FromMany_Execute()
        {
            using (var artist = SearchPage(typeof(ArtistDN), CheckLogin))
            {
                artist.Results.OrderBy("Id");

                artist.Results.SelectRow(0, 1);

                using (var process = artist.Results.EntityContextMenu(1).ConstructFromPopup<ProcessDN>(ArtistOperation.AssignPersonalAward))
                {
                    process.ExecuteAjax(ProcessOperation.Execute);

                    selenium.Wait(() => selenium.IsElementPresent(process.PopupVisibleLocator) &&
                        !process.OperationEnabled(ProcessOperation.Execute) &&
                        !process.OperationEnabled(ProcessOperation.Cancel) &&
                        !process.OperationEnabled(ProcessOperation.Suspend));
                } 
            }
        }

        [TestMethod]
        public void OperationCtx011_FromMany_Delete()
        {
            using (var albums = SearchPage(typeof(AlbumDN), CheckLogin))
            {
                int count = AuthLogic.Disable().Using(d => Database.Query<AlbumDN>().Count());

                //Order by Id descending so we delete the last cloned album
                albums.Search();

                Assert.AreEqual(count, albums.Results.RowsCount());

                albums.Results.SelectRow(0, 1);

                using (var process = albums.Results.SelectedClick().DeleteProcessClick(AlbumOperation.Delete))
                {
                    process.ExecuteAjax(ProcessOperation.Execute);

                    selenium.Wait(() => selenium.IsElementPresent(process.PopupVisibleLocator) &&
                        !process.OperationEnabled(ProcessOperation.Execute) &&
                        !process.OperationEnabled(ProcessOperation.Cancel) &&
                        !process.OperationEnabled(ProcessOperation.Suspend));
                }

                selenium.Search();

                Assert.AreEqual(count - 2, albums.Results.RowsCount());
            }
        }
    }
}
