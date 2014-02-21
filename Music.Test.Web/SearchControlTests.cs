﻿using System;
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
using Signum.Entities;
using Signum.Utilities;
using Signum.Test.Environment;
using Signum.Entities.DynamicQuery;

namespace Music.Test.Web
{
    [TestClass]
    public class SearchControlTests : Common
    {
        public SearchControlTests()
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

        void OrderById(string prefix = null)
        {
            selenium.Click(SearchTestExtensions.TableHeaderSelector(3, prefix));
            SearchTestExtensions.WaitSearchCompleted(selenium, prefix);
        }

        [TestMethod]
        public void SearchControl001_Filters()
        {
            using (var albums = SearchPage(typeof(AlbumDN), CheckLogin))
            {
                albums.Results.OrderBy("Id");
                Assert.AreEqual(12, albums.Results.RowsCount());

                albums.SearchControl.AddQuickFilter(0, 3);
                albums.Search();
                Assert.AreEqual(4, albums.Results.RowsCount());

                albums.Filters.QueryTokenBuilder.SelectToken("Label");
                albums.Filters.AddFilter().EntityLine().Find().SelectByPosition(0);
                albums.Search();
                Assert.AreEqual(2, albums.Results.RowsCount());

                albums.Filters.GetFilter(1).Delete();
                albums.Filters.AddFilter("Label.Name", FilterOperation.EqualTo, "virgin");
                albums.Search();
                Assert.AreEqual(2, albums.Results.RowsCount());

                albums.SearchControl.AddQuickFilter(0, 5);
                selenium.Search();
                Assert.AreEqual(1, albums.Results.RowsCount());

                albums.Filters.GetFilter(2).Delete();
                albums.Search();
                Assert.AreEqual(2, albums.Results.RowsCount());

                albums.Filters.AddFilter("Entity.Songs.Count", FilterOperation.GreaterThan, 1);
                albums.Search();
                Assert.AreEqual(1, albums.Results.RowsCount());

                albums.Filters.GetFilter(0).Delete();
                albums.Filters.GetFilter(1).Delete();
                albums.Filters.GetFilter(2).Delete();
                albums.Search();
                Assert.AreEqual(12, albums.Results.RowsCount());

                albums.SearchControl.AddQuickFilter(4);
                albums.Search();
                Assert.AreEqual(0, albums.Results.RowsCount());

                albums.Filters.GetFilter(0).EntityLine().Find().SelectByPosition(0);
                albums.Search();
                Assert.AreEqual(2, albums.Results.RowsCount());

                albums.Filters.GetFilter(0).Delete();
                albums.Pagination.SetElementsPerPage(5);
                Assert.AreEqual(5, albums.Results.RowsCount());
                Assert.IsTrue(selenium.IsElementPresent("jq=.sf-pagination-left:contains('5')")); ;
            }
        }

        [TestMethod]
        public void SearchControl002_FiltersInPopup()
        {
            using (var band = NormalPage<BandDN>(CheckLogin))
            {
                using (var artists = band.EntityList(a => a.Members).Find())
                {
                    artists.Results.OrderBy("Id");
                    Assert.AreEqual(8, artists.Results.RowsCount());

                    artists.SearchControl.AddQuickFilter(0, 4);
                    artists.SearchControl.AddQuickFilter(0, 5);
                    artists.Search();
                    Assert.AreEqual(7, artists.Results.RowsCount());

                    artists.SearchControl.AddQuickFilter(3, 2);
                    artists.Filters.GetFilter(2).Operation = FilterOperation.GreaterThan;
                    artists.Search();
                    Assert.AreEqual(3, artists.Results.RowsCount());

                    artists.Filters.GetFilter(2).Delete();
                    artists.Search();
                    Assert.AreEqual(7, artists.Results.RowsCount());

                    artists.Filters.QueryTokenBuilder.SelectToken("Entity");
                    artists.Filters.AddFilter().EntityLine().Find().SelectByPosition(0);
                    artists.Search();
                    Assert.AreEqual(1, artists.Results.RowsCount());

                    artists.Filters.GetFilter(2).Delete();
                    artists.Filters.AddFilter("Entity.Name", FilterOperation.EndsWith, "a");
                    artists.Search();
                    Assert.AreEqual(1, artists.Results.RowsCount());

                    artists.Filters.GetFilter(0).Delete();
                    artists.Filters.GetFilter(1).Delete();
                    artists.Filters.GetFilter(2).Delete();
                    artists.Search();
                    Assert.AreEqual(8, artists.Results.RowsCount());

                    artists.SearchControl.AddQuickFilter(2).Do(f =>
                    {
                        f.Operation = FilterOperation.LessThanOrEqual;
                        f.ValueLine().StringValue = "2";
                    });
                    artists.Search();
                    Assert.AreEqual(2, artists.Results.RowsCount());

                    artists.Filters.GetFilter(0).Delete();

                    artists.Pagination.SetElementsPerPage(5);
                    Assert.AreEqual(5, artists.Results.RowsCount());
                }
            }
        }

        [TestMethod]
        public void SearchControl003_Orders()
        {
            using (var albums = SearchPage(typeof(AlbumDN), CheckLogin))
            {
                int authorCol = 3;

                albums.Results.OrderBy(authorCol);
                Assert.AreEqual(Lite.Create<AlbumDN>(5), albums.Results.EntityInIndex(0));

                int labelCol = 4;

                albums.Results.ThenBy(labelCol);
                Assert.AreEqual(Lite.Create<AlbumDN>(7), albums.Results.EntityInIndex(0));
                Assert.IsTrue(albums.Results.IsHeaderMarkedSorted(authorCol, OrderType.Ascending));

                albums.Results.ThenByDescending(labelCol);
                Assert.AreEqual(Lite.Create<AlbumDN>(5), albums.Results.EntityInIndex(0));
                Assert.IsTrue(albums.Results.IsHeaderMarkedSorted(authorCol, OrderType.Ascending));

                albums.Results.OrderBy(labelCol);
                var first = Database.Query<AlbumDN>().OrderBy(a => a.Label.Name).Select(a => a.ToLite()).First();
                Assert.AreEqual(first /* Lite.Create<AlbumDN>(12)*/, albums.Results.EntityInIndex(0));
                Assert.IsFalse(albums.Results.IsHeaderMarkedSorted(authorCol, OrderType.Ascending));
            }
        }

        [TestMethod]
        public void SearchControl004_OrdersInPopup()
        {
            using (var band = NormalPage<BandDN>(CheckLogin))
            {
                using (var artists = band.EntityList(b => b.Members).Find())
                {
                    int isMaleColumn = 4;
                    artists.Results.OrderBy(isMaleColumn);
                    Assert.IsTrue(artists.Results.IsElementInCell(0, isMaleColumn, "input:checkbox[value=false]"));

                    artists.Results.OrderByDescending(isMaleColumn);
                    Assert.IsTrue(artists.Results.IsElementInCell(0, isMaleColumn, "input:checkbox[value=true]"));

                    int nameCol = 3;
                    artists.Results.ThenBy(nameCol);
                    Assert.AreEqual(Lite.Create<ArtistDN>(1), artists.Results.EntityInIndex(0));
                    Assert.IsTrue(artists.Results.IsHeaderMarkedSorted(isMaleColumn, OrderType.Descending));

                    artists.Results.ThenByDescending(nameCol);
                    Assert.AreEqual(Lite.Create<ArtistDN>(8), artists.Results.EntityInIndex(0));
                    Assert.IsTrue(artists.Results.IsHeaderMarkedSorted(isMaleColumn, OrderType.Descending));

                    artists.Results.OrderBy(2);
                    Assert.AreEqual(Lite.Create<ArtistDN>(1), artists.Results.EntityInIndex(0));
                    Assert.IsFalse(artists.Results.IsHeaderMarkedSorted(isMaleColumn));
                    Assert.IsFalse(artists.Results.IsHeaderMarkedSorted(nameCol));
                }
            }
        }

        [TestMethod]
        public void SearchControl005_UserColumns()
        {
            using (var albums = SearchPage(typeof(AlbumDN), CheckLogin))
            {
                albums.SearchControl.AddColumn("Label.Id");
                albums.SearchControl.AddColumn("Label.Name");

                albums.Results.EditColumnName(7, "Label Id");

                albums.Results.OrderBy("Id");

                albums.Results.MoveLeft(7);
                albums.Results.MoveRight(6);

                albums.Results.RemoveColumn(7);
                selenium.Wait(() => albums.Results.RowsCount() == 0);
                albums.Search();

                selenium.AssertElementNotPresent(albums.Results.HeaderCellLocator(8));
            }
        }

        [TestMethod]
        public void SearchControl006_UserColumnsInPopup()
        {
            using (var band = NormalPage<BandDN>(CheckLogin))
            {
                using (var artists = band.EntityList(b=> b.Members).Find())
                {
                    artists.Results.EditColumnName(5, "Male");
                    artists.Results.MoveRight(2);
                    artists.Results.MoveLeft(3);

                    artists.Results.RemoveColumn(3);
                    selenium.Wait(() => artists.Results.RowsCount() == 0);
                    artists.Search();

                    selenium.AssertElementNotPresent(artists.Results.HeaderCellLocator(7));
                }
            }
        }

        [TestMethod]
        public void SearchControl007_ImplementedByFinder()
        {
            using (var authors = SearchPage(typeof(IAuthorDN), CheckLogin))
            {
                authors.Results.OrderBy("Id");
                Assert.AreEqual(Lite.Create<ArtistDN>(1), authors.Results.EntityInIndex(0));
                Assert.AreEqual(Lite.Create<BandDN>(1), authors.Results.EntityInIndex(1));

                authors.Filters.AddFilter("Id", FilterOperation.EqualTo, 1);
                authors.Search();
                Assert.IsTrue(authors.Results.RowsCount() == 2);

                authors.Filters.GetFilter(0).Delete();
                authors.Filters.AddFilter("Entity.(Artist).Id", FilterOperation.EqualTo, 1);
                authors.Search();
                Assert.IsTrue(authors.Results.RowsCount() == 1);

                using (var artist = authors.CreateChoose<ArtistDN>())
                {
                    selenium.AssertElementPresent(artist.ValueLine(a => a.Dead).Prefix);
                }
            }
        }

        [TestMethod]
        public void SearchControl008_MultiplyFinder()
        {
            using(var artists = SearchPage(typeof(ArtistDN), CheckLogin))
            {
                Assert.IsFalse(artists.Filters.IsAddFilterEnabled);
                Assert.IsFalse(artists.SearchControl.IsAddColumnEnabled);

                artists.Results.OrderBy("Id");
                Assert.AreEqual(8, artists.Results.RowsCount());

                artists.Filters.QueryTokenBuilder.SelectToken("Entity");
                Assert.IsTrue(artists.Filters.IsAddFilterEnabled);
                Assert.IsTrue(artists.SearchControl.IsAddColumnEnabled);

                artists.Filters.QueryTokenBuilder.SelectToken("Entity.Friends");
                Assert.IsFalse(artists.Filters.IsAddFilterEnabled);
                Assert.IsFalse(artists.SearchControl.IsAddColumnEnabled);

                artists.Filters.QueryTokenBuilder.SelectToken("Entity.Friends.Count");
                Assert.IsTrue(artists.Filters.IsAddFilterEnabled);
                Assert.IsTrue(artists.SearchControl.IsAddColumnEnabled);

                artists.Filters.AddFilter("Entity.Friends.Count", FilterOperation.EqualTo, 1);
                artists.Search();
                Assert.IsFalse(artists.Results.HasMultiplyMessage);
                Assert.AreEqual(3, artists.Results.RowsCount());

                artists.SearchControl.AddColumn("Entity.Friends.Count");
                artists.Search();
                Assert.IsFalse(artists.Results.HasMultiplyMessage);
                Assert.AreEqual(3, artists.Results.RowsCount());

                artists.Filters.QueryTokenBuilder.SelectToken("Entity.Friends");
                Assert.IsFalse(artists.Filters.IsAddFilterEnabled);
                Assert.IsFalse(artists.SearchControl.IsAddColumnEnabled);

                artists.Filters.GetFilter(0).Delete();
                artists.Results.RemoveColumn(8);
                artists.Filters.QueryTokenBuilder.SelectToken("Entity.Friends.Element");
                Assert.IsTrue(artists.Filters.IsAddFilterEnabled);
                Assert.IsTrue(artists.SearchControl.IsAddColumnEnabled);

                artists.SearchControl.AddColumn("Entity.Friends.Element");
                artists.Search();
                Assert.IsTrue(artists.Results.HasMultiplyMessage);
                Assert.AreEqual(10, artists.Results.RowsCount());

                artists.Filters.AddFilter().EntityLine().Find().SelectByPositionOrderById(2);
                artists.Search();
                Assert.IsTrue(artists.Results.HasMultiplyMessage);
                Assert.AreEqual(3, artists.Results.RowsCount());

                artists.Filters.GetFilter(0).Delete();
                artists.Results.RemoveColumn(8);
                artists.Filters.QueryTokenBuilder.SelectToken("Entity.Friends.Any");
                Assert.IsTrue(artists.Filters.IsAddFilterEnabled);
                Assert.IsFalse(artists.SearchControl.IsAddColumnEnabled);

                artists.Filters.AddFilter().EntityLine().Find().SelectByPositionOrderById(2);
                artists.Search();
                Assert.IsFalse(artists.Results.HasMultiplyMessage);
                Assert.AreEqual(3, artists.Results.RowsCount());

                artists.Filters.AddFilter("Entity.Friends.Any.Name", FilterOperation.Contains, "i");
                artists.Search();
                Assert.IsFalse(artists.Results.HasMultiplyMessage);
                Assert.AreEqual(0, artists.Results.RowsCount());

                artists.Filters.GetFilter(1).ValueLine().StringValue = "arcy";
                artists.Search();
                Assert.IsFalse(artists.Results.HasMultiplyMessage);
                Assert.AreEqual(3, artists.Results.RowsCount());

                artists.Filters.GetFilter(0).Delete();
                artists.Filters.GetFilter(1).Delete();
                artists.Filters.QueryTokenBuilder.SelectToken("Entity.Friends.All");
                Assert.IsTrue(artists.Filters.IsAddFilterEnabled);
                Assert.IsFalse(artists.SearchControl.IsAddColumnEnabled);

                artists.Filters.AddFilter().Do(a =>
                {
                    a.Operation = FilterOperation.DistinctTo;
                    a.EntityLine().Find().SelectByPositionOrderById(2);
                });
                artists.Search();
                Assert.IsFalse(artists.Results.HasMultiplyMessage);
                Assert.AreEqual(5, artists.Results.RowsCount());

                artists.Filters.AddFilter("Entity.Friends.All.Name", FilterOperation.Contains, "Corgan");
                artists.Search();

                Assert.IsFalse(artists.Results.HasMultiplyMessage);
                Assert.AreEqual(4, artists.Results.RowsCount());
            }
        }
    }
}
