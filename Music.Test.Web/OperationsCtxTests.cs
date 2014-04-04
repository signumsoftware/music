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
using Signum.Engine.Operations;

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
            Login();
            using (var artists = SearchPage(typeof(ArtistDN)))
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
            Login();
            SearchPage(typeof(BandDN)).EndUsing(bands =>
            {
                bands.SearchControl.Search();

                bands.SearchControl.Results.EntityContextMenu(0).ConstructFromPopup<AlbumDN>(AlbumOperation.CreateAlbumFromBand).Using(album =>
                {
                    album.ValueLineValue(a => a.Name, "ctxtest");
                    album.ValueLineValue(a => a.Year, DateTime.Now.Year);
                    album.EntityLine(a => a.Label).Find().SelectByPosition(0);
                    return album.OkWaitPopupControl<AlbumDN>();
                }).EndUsing(album =>
                {
                    Assert.IsTrue(album.HasId());
                    album.RuntimeInfo().ToLite().Delete();
                });
            }); 
        }

        [TestMethod]
        public void OperationCtx003_Delete()
        {
            Login();
            using (var albums = SearchPage(typeof(AlbumDN)))
            {
                CreateAlbum("blasco");

                int count = Database.Query<AlbumDN>().Count();

                albums.Results.OrderByDescending("Id");

                Assert.AreEqual(count, albums.Results.RowsCount());

                albums.Results.EntityContextMenu(0).ExecuteClick(AlbumOperation.Delete);

                albums.Selenium.ConsumeConfirmation();

                albums.Search();
                albums.Search();

                Assert.AreEqual(count - 1, albums.Results.RowsCount());
            }
        }

        private static void CreateAlbum(string name)
        {
            using (OperationLogic.AllowSave<AlbumDN>())
                new AlbumDN
                {
                    Year = 2000,
                    Name = name,
                    Author = Database.Query<ArtistDN>().First(),
                    State = AlbumState.Saved
                }.Save();
        }

        [TestMethod]
        public void OperationCtx004_ConstructFromMany()
        {
            Login();
            SearchPage(typeof(AlbumDN)).EndUsing(albums =>
            {
                albums.Search();
                albums.Results.SelectRow(0, 1);

                using (var album = albums.Results.EntityContextMenu(1).ConstructFromPopup<AlbumDN>(AlbumOperation.CreateGreatestHitsAlbum))
                {
                    album.ValueLineValue(a => a.Name, "test greatest hits");
                    album.EntityCombo(a => a.Label).SelectLabel("Virgin");
                    album.ExecuteAjax(AlbumOperation.Save);
                    Assert.IsTrue(album.RuntimeInfo().IdOrNull.HasValue);
                    album.RuntimeInfo().ToLite().Delete();
                }
            });
        }

        [TestMethod]
        public void OperationCtx010_FromMany_Execute()
        {
            Login();

            ProcessViewStart();

            using (var artist = SearchPage(typeof(ArtistDN)))
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
            Login();

            ProcessViewStart();

            using (var albums = SearchPage(typeof(AlbumDN)))
            {
                CreateAlbum("alb1");
                CreateAlbum("alb2");

                int count = Database.Query<AlbumDN>().Count();

                albums.Results.OrderByDescending("Id");

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


        private void ProcessViewStart()
        {
            string url = Url("Process/View");

            selenium.Open(url);
            selenium.WaitForPageToLoad();

            if (selenium.IsElementPresent("jq=#processMainDiv span:contains('STOPPED')"))
            {
                selenium.Click("jq=#processMainDiv a:contains('Start')");

                selenium.Wait(() =>
                {
                    selenium.Open(url);
                    selenium.WaitForPageToLoad();

                    return !selenium.IsElementPresent("jq=#processMainDiv span:contains('STOPPED')");
                });
            }
        }
    }
}
