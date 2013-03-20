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
            string pathAlbumSearch = FindRoute("Album");

            CheckLoginAndOpen(pathAlbumSearch);

            //add filter of simple value
            selenium.FilterSelectToken(0, "label=Year", false);
            selenium.AddFilter(0);
            selenium.FilterSelectOperation(0, "value=GreaterThan");
            selenium.Type("value_0", "2000");

            //add filter of lite
            string filterPrefix = "value_1_";
            selenium.FilterSelectToken(0, "label=Label", true);
            selenium.AddFilter(0);
            selenium.LineFind(filterPrefix);
            selenium.Search(filterPrefix);
            selenium.SelectEntityRow(Lite.Create<LabelDN>(1), filterPrefix);
            selenium.PopupOk(filterPrefix);

            //add user column
            selenium.FilterSelectToken(1, "label=Owner", true);
            selenium.AddColumn("Label.Owner");

            int yearCol = 6;

            //add order
            selenium.Sort(yearCol, true);
            selenium.Sort(yearCol, false);
            
            string uqMenuId = "tmUserQueries";
            string uqCreateId = "qbUserQueryNew";

            //create user query
            selenium.QueryMenuOptionClick(uqMenuId, uqCreateId);
            selenium.WaitForPageToLoad(PageLoadTimeout);
            selenium.Type("DisplayName", "Last albums");

            selenium.EntityOperationClick(UserQueryOperation.Save);
            selenium.WaitForPageToLoad(PageLoadTimeout);

            //check new user query is in the dropdownlist
            string uqOptionSelector = "title='Last albums'";
            selenium.Open(pathAlbumSearch);
            selenium.WaitForPageToLoad(PageLoadTimeout);
            selenium.QueryMenuOptionPresentByAttr(uqMenuId, uqOptionSelector, true);

            //load user query
            selenium.Click(SearchTestExtensions.QueryMenuOptionLocatorByAttr(uqMenuId, uqOptionSelector));
            selenium.WaitForPageToLoad(PageLoadTimeout);
            //Filter present
            Assert.IsTrue(selenium.IsElementPresent("value_0"));
            Assert.IsTrue(selenium.IsElementPresent(LinesTestExtensions.EntityLineToStrSelector("value_1_")));
            //Column present
            selenium.TableHasColumn("Label.Owner");
            //Sort present
            selenium.TableHeaderMarkedAsSorted(yearCol, false, true);
        }

        [TestMethod]
        public void UserQueries002_Edit()
        {
            var uqName = "uq" + DateTime.Now.Ticks.ToString().Substring(8);
            var userQuery = CreateAlbumUserQuery(uqName);

            string pathAlbumSearch = FindRoute("Album");

            CheckLoginAndOpen(pathAlbumSearch);

            string uqMenuId = "tmUserQueries";
            string uqOptionSelector = "title='" + uqName + "'";
            string editId = "qbUserQueryEdit";

            //load user query
            selenium.Click(SearchTestExtensions.QueryMenuOptionLocatorByAttr(uqMenuId, uqOptionSelector));
            selenium.WaitForPageToLoad(PageLoadTimeout);

            //edit it
            selenium.QueryMenuOptionClick(uqMenuId, editId);
            selenium.WaitForPageToLoad(PageLoadTimeout);
            //remove filter
            selenium.LineRemove("Filters_0_");
            //add column
            selenium.LineCreate("Columns_", false, 0);
            string prefix = "Columns_0_";
            selenium.WaitAjaxFinished(() => selenium.IsElementPresent(prefix + "DisplayName"));
            selenium.Type(prefix + "DisplayName", "Label owner's country");
            selenium.FilterSelectToken(0, "value=Label", true, prefix);
            selenium.FilterSelectToken(1, "value=Owner", true, prefix);
            selenium.FilterSelectToken(2, "value=Country", true, prefix);

            //save it
            selenium.EntityOperationClick(UserQueryOperation.Save);
            selenium.WaitForPageToLoad(PageLoadTimeout);

            //check new user query is in the dropdownlist
            selenium.Open(pathAlbumSearch);
            selenium.WaitForPageToLoad(PageLoadTimeout);
            
            //load user query
            selenium.Click(SearchTestExtensions.QueryMenuOptionLocatorByAttr(uqMenuId, uqOptionSelector));
            selenium.WaitForPageToLoad(PageLoadTimeout);
            //Filter deleted
            Assert.IsFalse(selenium.IsElementPresent("value_0"));
            //New column present
            selenium.TableHasColumn("Label.Owner.Country");
        }

        [TestMethod]
        public void UserQueries003_Delete()
        {
            string pathAlbumSearch = FindRoute("Album");

            string uqMenuId = "tmUserQueries";
            string uqCreateId = "qbUserQueryNew";
            string editId = "qbUserQueryEdit";

            CheckLoginAndOpen(pathAlbumSearch);

            int yearCol = 6;

            //add order
            selenium.Sort(yearCol, true);
            
            //create user query
            selenium.QueryMenuOptionClick(uqMenuId, uqCreateId);
            selenium.WaitForPageToLoad(PageLoadTimeout);
            selenium.Type("DisplayName", "test");

            selenium.EntityOperationClick(UserQueryOperation.Save);
            selenium.WaitForPageToLoad(PageLoadTimeout);

            //check new user query is in the dropdownlist
            string uqOptionSelector = "title='test'";
            selenium.Open(pathAlbumSearch);
            selenium.WaitForPageToLoad(PageLoadTimeout);
            selenium.QueryMenuOptionPresentByAttr(uqMenuId, uqOptionSelector, true);

            //load user query
            selenium.Click(SearchTestExtensions.QueryMenuOptionLocatorByAttr(uqMenuId, uqOptionSelector));
            selenium.WaitForPageToLoad(PageLoadTimeout);
            //Sort present
            selenium.TableHeaderMarkedAsSorted(yearCol, true, true);

            //edit it
            selenium.QueryMenuOptionClick(uqMenuId, editId);
            selenium.WaitForPageToLoad(PageLoadTimeout);

            //remove it
            selenium.EntityOperationClick(UserQueryOperation.Delete);
            Assert.IsTrue(Regex.IsMatch(selenium.GetConfirmation(), ".*"));
            selenium.WaitForPageToLoad(PageLoadTimeout);
            selenium.QueryMenuOptionPresentByAttr(uqMenuId, uqOptionSelector, false);
        }

        public static UserQueryDN CreateAlbumUserQuery(string userQueryName)
        {
            using (AuthLogic.UnsafeUserSession("su"))
            {
                return new UserQueryDN(typeof(AlbumDN))
                {
                    Related = Database.Query<UserDN>().Where(u => u.UserName == "internal").Select(a => a.ToLite<IdentifiableEntity>()).SingleEx(),
                    DisplayName = userQueryName,
                    Filters = { new QueryFilterDN("Id", 3) { Operation = FilterOperation.GreaterThan } },
                }.ParseAndSave();
            }
        }
    }
}
