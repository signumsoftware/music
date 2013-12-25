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
using Signum.Utilities;
using Signum.Entities.UserQueries;
using System.Text.RegularExpressions;
using Signum.Entities;
using Signum.Test.Environment;
using Signum.Entities.DynamicQuery;
using Signum.Engine.UserQueries;

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
            SearchPage(typeof(AlbumDN), CheckLogin).Using(album =>
            {
                album.Filters.AddFilter("Year", FilterOperation.GreaterThan, 2000);
                album.Filters.AddFilter("Label", FilterOperation.EqualTo, Lite.Create<LabelDN>(1));
                album.SearchControl.AddColumn("Label.Owner");

                album.Results.OrderBy(6);
                album.Results.OrderByDescending(6);

                return album.SearchControl.NewUserQuery();
            })
            .Using(uq =>
            {
                uq.ValueLineValue(a => a.DisplayName, "Last albums");
                uq.ExecuteAjax(UserQueryOperation.Save);

                return SearchPage(typeof(AlbumDN));
            })
            .EndUsing(albums =>
            {
                albums.SearchControl.UserQueryLocatorClick("Last albums");
                albums.Selenium.WaitElementPresent(albums.Filters.GetFilter(0).ValueLine().Prefix);
                Assert.IsTrue(albums.Results.HasColumn("Label.Owner"));
                Assert.IsTrue(albums.Results.IsHeaderMarkedSorted(6, OrderType.Descending));
            }); 
        }

        [TestMethod]
        public void UserQueries002_Edit()
        {
            var uqName = "uq" + DateTime.Now.Ticks.ToString().Substring(8);
            var userQuery = AuthLogic.UnsafeUserSession("su").Using(_ => 
                new UserQueryDN(typeof(AlbumDN))
                {
                    Related = Database.Query<UserDN>().Where(u => u.UserName == "internal").Select(a => a.ToLite<IdentifiableEntity>()).SingleEx(),
                    DisplayName = uqName,
                    Filters = { new QueryFilterDN { Token = new QueryTokenDN("Id"), Operation = FilterOperation.GreaterThan, Value = 3 } },
                }.ParseAndSave());

            SearchPage(typeof(AlbumDN), CheckLogin).Using(albums =>
            {
                albums.SearchControl.UserQueryLocatorClick(uqName);
                return albums.SearchControl.EditUserQuery();
            }).Using(uq =>
            {
                uq.EntityRepeater(a => a.Filters).Remove(0);
                uq.EntityRepeater(a => a.Columns).Create();
                uq.EntityRepeater(a => a.Columns).Details<QueryColumnDN>(0).Do(qc =>
                {
                    qc.ValueLineValue(a => a.DisplayName, "Label owner's country");
                    qc.QueryTokenBuilder(a => a.Token).SelectToken("Label.Owner.Country");
                });
                uq.ExecuteAjax(UserQueryOperation.Save);

                return SearchPage(typeof(AlbumDN));
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
            SearchPage(typeof(AlbumDN), CheckLogin).Using(albums =>
            {
                albums.Results.OrderBy(5);
                return albums.SearchControl.NewUserQuery();
            })
            .Using(uq =>
            {
                uq.ValueLineValue(a => a.DisplayName, "test");
                uq.ExecuteSubmit(UserQueryOperation.Save);

                return SearchPage(typeof(AlbumDN));
            }).Using(albums =>
            {
                albums.SearchControl.UserQueryLocatorClick("test");
                Assert.IsTrue(albums.Results.IsHeaderMarkedSorted(5, OrderType.Ascending));
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
