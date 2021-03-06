﻿using System;
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
using Signum.Entities.UserQueries;
using System.Text.RegularExpressions;
using Signum.Entities;
using Signum.Test.Environment;
using Signum.Entities.DynamicQuery;
using Signum.Engine.UserQueries;
using Signum.Entities.UserAssets;

namespace Music.Test.Web
{
    [TestClass]
    public class UserQueriesTests : Common
    {
        public UserQueriesTests()
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
        public void UserQueries001_Create()
        {
            Login(); 

            SearchPage(typeof(AlbumEntity)).Using(album =>
            {
                album.Filters.AddFilter("Year", FilterOperation.GreaterThan, 2000);
                album.Filters.AddFilter("Label", FilterOperation.EqualTo, Lite.Create<LabelEntity>(1));
                album.SearchControl.AddColumn("Label.Owner");

                album.Results.OrderBy("Label.Owner");
                album.Results.OrderByDescending("Label.Owner");

                return album.SearchControl.NewUserQuery();
            })
            .Using(uq =>
            {
                uq.ValueLineValue(a => a.DisplayName, "Last albums");
                uq.ExecuteAjax(UserQueryOperation.Save);

                return SearchPage(typeof(AlbumEntity));
            })
            .EndUsing(albums =>
            {
                albums.SearchControl.UserQueryLocatorClick("Last albums");
                albums.Selenium.WaitElementPresent(albums.Filters.GetFilter(0).ValueLine().Locator);
                Assert.IsTrue(albums.Results.HasColumn("Label.Owner"));
                Assert.IsTrue(albums.Results.IsHeaderMarkedSorted("Label.Owner", OrderType.Descending));
            }); 
        }

        [TestMethod]
        public void UserQueries002_Edit()
        {
            Login();

            var uqName = "uq" + DateTime.Now.Ticks.ToString().Substring(8);
            var userQuery = AuthLogic.UnsafeUserSession("su").Using(_ => 
                new UserQueryEntity(typeof(AlbumEntity))
                {
                    Owner = Database.Query<UserEntity>().Where(u => u.UserName == "internal").Select(a => a.ToLite<Entity>()).SingleEx(),
                    DisplayName = uqName,
                    Filters = { new QueryFilterEntity { Token = new QueryTokenEntity("Id"), Operation = FilterOperation.GreaterThan, ValueString = "3" } },
                }.ParseAndSave());

            SearchPage(typeof(AlbumEntity)).Using(albums =>
            {
                albums.SearchControl.UserQueryLocatorClick(uqName);
                return albums.SearchControl.EditUserQuery();
            }).Using(uq =>
            {
                uq.EntityRepeater(a => a.Filters).Remove(0);
                uq.EntityRepeater(a => a.Columns).CreateElement<QueryColumnEntity>().Do(qc =>
                {
                    qc.ValueLineValue(a => a.DisplayName, "Label owner's country");
                    qc.QueryTokenBuilder(a => a.Token).SelectToken("Label.Owner.Country");
                });
                uq.ExecuteAjax(UserQueryOperation.Save);

                return SearchPage(typeof(AlbumEntity));
            })
            .EndUsing(albums =>
            {
                albums.SearchControl.UserQueryLocatorClick(uqName);
                selenium.AssertElementNotPresent(albums.Filters.GetFilter(0).OperationLocator);
                Assert.IsTrue(albums.Results.HasColumn("Label.Owner.Country"));
            });

        }

        [TestMethod]
        public void UserQueries003_Delete()
        {
            Login();

            SearchPage(typeof(AlbumEntity)).Using(albums =>
            {
                albums.Results.OrderBy("Year");
                return albums.SearchControl.NewUserQuery();
            })
            .Using(uq =>
            {
                uq.ValueLineValue(a => a.DisplayName, "test");
                uq.ExecuteSubmit(UserQueryOperation.Save);

                return SearchPage(typeof(AlbumEntity));
            }).Using(albums =>
            {
                albums.SearchControl.UserQueryLocatorClick("test");
                selenium.Wait(() => albums.Results.IsHeaderMarkedSorted("Year", OrderType.Ascending));
                return albums.SearchControl.EditUserQuery();
            }).Using(uq =>
            {
                return uq.DeleteSubmit(UserQueryOperation.Delete);
            }).EndUsing(albums =>
            {
                selenium.AssertElementNotPresent(albums.SearchControl.UserQueryLocator("test"));
            }); 
        }

    }
}
