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
using Signum.Entities.Reports;
using Signum.Engine.Basics;
using Signum.Entities;
using Signum.Entities.DynamicQuery;
using Signum.Engine.DynamicQuery;
using Signum.Entities.UserQueries;
using Signum.Engine.UserQueries;
using Signum.Entities.ControlPanel;
using Signum.Test.Environment;

namespace Music.Test.Web
{
    [TestClass]
    public class ControlPanelTests : Common
    {
        public ControlPanelTests()
        {
            UserQueriesTests.CreateAlbumUserQuery("test");
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
        public void ControlPanel001_Create()
        {
            CheckLoginAndOpen(FindRoute("ControlPanel"));

            selenium.SearchCreate();
            selenium.WaitForPageToLoad(PageLoadTimeout);

            //Related is RoleDN.Current, and when available UserDN.Current
            selenium.Type("DisplayName", "Control Panel Home Page");
            selenium.Click("HomePagePriority");
            selenium.Type("NumberOfColumns", "2");

            selenium.EntityOperationClick(ControlPanelOperation.Save);
            selenium.WaitForPageToLoad(PageLoadTimeout);

            string partsPrefix = "Parts_";

            //SearchControlPart
            CreateNewPart("UserQueryPart");
            string part0 = partsPrefix + "0_";
            selenium.Type(part0 + "Title", "Last Albums");
            selenium.LineFindAndSelectElements(part0 + "Content_UserQuery_", new int[]{ 0 });

            //CountSearchControlPart
            CreateNewPart("CountSearchControlPart");
            string part1 = partsPrefix + "1_";
            selenium.Type(part1 + "Title", "My Count Controls");
            string part1ContentUQsPrefix = part1 + "Content_UserQueries_";
            selenium.LineCreate(part1ContentUQsPrefix, false, 0);
            selenium.RepeaterWaitUntilItemLoaded(part1ContentUQsPrefix, 0);
            selenium.LineFindAndSelectElements(part1ContentUQsPrefix + "0_UserQuery_", new int[] { 0 });
            
            //LinkListPart - drag to second column
            CreateNewPart("LinkListPart");
            string part2 = partsPrefix + "2_";
            selenium.Type(part2 + "Title", "My Links");
            CreateLinkListPartItem(selenium, part2, 0, "Best Band", "http://localhost/Music.Web/View/Band/1");
            CreateLinkListPartItem(selenium, part2, 1, "Best Artist", "http://localhost/Music.Web/View/Artist/1");
            
            selenium.DragAndDropToObject("jq=#sfCpAdminContainer td[data-column=0] .sf-ftbl-part:eq(1)",
                "jq=#sfCpAdminContainer td[data-column=1] .sf-ftbl-droppable");

            selenium.EntityOperationClick(ControlPanelOperation.Save);
            selenium.MainEntityHasId();
        }

        void CreateNewPart(string partType)
        {
            selenium.EntityButtonClick("CreatePart");

            selenium.WaitAjaxFinished(() => selenium.IsElementPresent(SeleniumExtensions.PopupSelector("New_")));
            selenium.Click(partType);
            selenium.WaitForPageToLoad(PageLoadTimeout);
        }

        string PartFrontEndSelector(int rowIndexBase1, int colIndexBase1)
        {
            return "jq=table > tbody > tr:nth-child({0}) > td:nth-child({1})".Formato(rowIndexBase1, colIndexBase1);
        }

        void CreateLinkListPartItem(ISelenium selenium, string partPrefix, int linkIndexBase0, string label, string link)
        {
            string partContentLinksPrefix = partPrefix + "Content_Links_";

            selenium.LineCreate(partContentLinksPrefix, false, 0);
            selenium.RepeaterWaitUntilItemLoaded(partContentLinksPrefix, 0);
            string partContentLinksItemPrefix = partContentLinksPrefix + linkIndexBase0 + "_";
            selenium.Type(partContentLinksItemPrefix + "Label", label);
            selenium.Type(partContentLinksItemPrefix + "Link", link);
        }
    }
}
