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
using OpenQA.Selenium;

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
            using (var artists = SearchPage(typeof(ArtistEntity)))
            {
                artists.Results.OrderBy("Id");

                selenium.Wait(() => selenium.IsElementPresent(artists.Results.RowLocator(1)));

                artists.SearchControl.Results.EntityContextMenu(0).ExecuteClick(ArtistOperation.AssignPersonalAward);

                Assert.IsTrue(artists.SearchControl.Results.EntityContextMenu(0).IsDisabled(ArtistOperation.AssignPersonalAward)); 
            }
        }

        [TestMethod]
        public void OperationCtx002_ConstructFrom_OpenPopup()
        {
            Login();
            SearchPage(typeof(BandEntity)).EndUsing(bands =>
            {
                bands.SearchControl.Search();

                bands.SearchControl.Results.EntityContextMenu(0).MenuClickPopup<AlbumEntity>(AlbumOperation.CreateAlbumFromBand).Using(album =>
                {
                    album.ValueLineValue(a => a.Name, "ctxtest");
                    album.ValueLineValue(a => a.Year, DateTime.Now.Year);
                    album.EntityLine(a => a.Label).Find().SelectByPosition(0);
                    return album.OkWaitPopupControl<AlbumEntity>();
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
            using (var albums = SearchPage(typeof(AlbumEntity)))
            {
                CreateAlbum("blasco");

                int count = Database.Query<AlbumEntity>().Count();

                albums.Results.OrderByDescending("Id");

                Assert.AreEqual(count, albums.Results.RowsCount());

                albums.Results.EntityContextMenu(0).DeleteClick(AlbumOperation.Delete);

                albums.Search();

                Assert.AreEqual(count - 1, albums.Results.RowsCount());
            }
        }

        private static void CreateAlbum(string name)
        {
            using (OperationLogic.AllowSave<AlbumEntity>())
                new AlbumEntity
                {
                    Year = 2000,
                    Name = name,
                    Author = Database.Query<ArtistEntity>().First(),
                    State = AlbumState.Saved
                }.Save();
        }

        [TestMethod]
        public void OperationCtx004_ConstructFromMany()
        {
            Login();
            SearchPage(typeof(AlbumEntity)).EndUsing(albums =>
            {
                albums.Search();
                albums.Results.SelectRow(0, 1);

                using (var album = albums.Results.EntityContextMenu(1).MenuClickPopup<AlbumEntity>(AlbumOperation.CreateGreatestHitsAlbum))
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

            using (var artist = SearchPage(typeof(ArtistEntity)))
            {
                artist.Results.OrderBy("Id");

                artist.Results.SelectRow(0, 1);

                using (var process = artist.Results.EntityContextMenu(1).MenuClickPopup<ProcessEntity>(ArtistOperation.AssignPersonalAward))
                {
                    process.ExecuteAjax(ProcessOperation.Execute);

                    selenium.Wait(() => selenium.IsElementPresent(process.PopupLocator) &&
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

            using (var albums = SearchPage(typeof(AlbumEntity)))
            {
                CreateAlbum("alb1");
                CreateAlbum("alb2");

                int count = Database.Query<AlbumEntity>().Count();

                albums.Results.OrderByDescending("Id");

                Assert.AreEqual(count, albums.Results.RowsCount());

                albums.Results.SelectRow(0, 1);

                using (var process = albums.Results.SelectedClick().DeleteProcessClick(AlbumOperation.Delete))
                {
                    process.ExecuteAjax(ProcessOperation.Execute);

                    selenium.Wait(() => selenium.IsElementPresent(process.PopupLocator) &&
                        !process.OperationEnabled(ProcessOperation.Execute) &&
                        !process.OperationEnabled(ProcessOperation.Cancel) &&
                        !process.OperationEnabled(ProcessOperation.Suspend));
                }

                albums.Search();

                Assert.AreEqual(count - 2, albums.Results.RowsCount());
            }
        }


        private void ProcessViewStart()
        {
            string url = Url("Process/View");

            selenium.Url = url;

            var div = selenium.FindElement(By.CssSelector("#processMainDiv"));
            if (div.ContainsText("STOPPED"))
            {
                var button = div.FindElements(By.CssSelector("a")).FirstEx(a => a.ContainsText("Start"));
                
                button.Click();

                selenium.Wait(() =>
                {
                    selenium.Url = url;

                    return !selenium.FindElement(By.CssSelector("#processMainDiv")).ContainsText("STOPPED");
                });
            }
        }
    }
}
