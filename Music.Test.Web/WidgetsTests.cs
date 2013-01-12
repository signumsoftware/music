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
using Signum.Entities.Notes;
using Signum.Entities.Alerts;

namespace Music.Test.Web
{
    [TestClass]
    public class WidgetsTests : Common
    {
        public WidgetsTests()
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
        public void Widgets001_QuickLinkFind()
        {
            CheckLoginAndOpen(ViewRoute("Label", 1));

            selenium.QuickLinkClick(1);
            
            string popupPrefix = "New_";
            selenium.WaitAjaxFinished(() => selenium.IsElementPresent(SeleniumExtensions.PopupSelector(popupPrefix)));
            selenium.WaitAjaxFinished(() => selenium.IsElementPresent(SearchTestExtensions.RowSelector(selenium, 1, popupPrefix)));

            selenium.Open(FindRoute("Label"));
            selenium.WaitForPageToLoad(PageLoadTimeout);
            selenium.Search();

            selenium.EntityContextMenu(1);
            selenium.EntityContextQuickLinkClick(1, 1);

            selenium.WaitAjaxFinished(() => selenium.IsElementPresent(SeleniumExtensions.PopupSelector(popupPrefix)));
            selenium.WaitAjaxFinished(() => selenium.IsElementPresent(SearchTestExtensions.RowSelector(selenium, 1, popupPrefix)));
        }

        [TestMethod]
        public void Widgets010_Notes()
        {
            CheckLoginAndOpen(ViewRoute("Label", 1));
            Assert.IsTrue(selenium.EntityHasNNotes(0));

            //Create note
            selenium.NotesCreateClick();

            string popupPrefix = "New_";
            selenium.WaitAjaxFinished(() => selenium.IsElementPresent(SeleniumExtensions.PopupSelector(popupPrefix)));

            selenium.Type(popupPrefix + "Text", "note test");
            selenium.EntityOperationClick(NoteOperation.Save);
            selenium.WaitAjaxFinished(() => !selenium.IsElementPresent(SeleniumExtensions.PopupSelector(popupPrefix)));

            selenium.GetAlert();
            selenium.WaitAjaxFinished(() => selenium.EntityHasNNotes(1));

            //View notes
            selenium.NotesViewClick();
            selenium.WaitAjaxFinished(() => selenium.IsElementPresent(SeleniumExtensions.PopupSelector(popupPrefix)));
            selenium.WaitAjaxFinished(() => selenium.IsElementPresent(SearchTestExtensions.RowSelector(selenium, 1, popupPrefix)));
        }

        [TestMethod]
        public void Widgets020_Alerts()
        {
            string viewRoute = ViewRoute("Label", 1);
            CheckLoginAndOpen(viewRoute);
            
            string warned = WidgetsTestExtensions.AlertWarnedClass;
            string future = WidgetsTestExtensions.AlertFutureClass;
            string attended = WidgetsTestExtensions.AlertAttendedClass;

            Assert.IsTrue(selenium.EntityHasNAlerts(0, warned));
            Assert.IsTrue(selenium.EntityHasNAlerts(0, future));
            Assert.IsTrue(selenium.EntityHasNAlerts(0, attended));

            //Create future
            selenium.AlertsCreateClick();

            string popupPrefix = "New_";
            selenium.WaitAjaxFinished(() => selenium.IsElementPresent(SeleniumExtensions.PopupSelector(popupPrefix)));

            selenium.Type(popupPrefix + "Text", "alert test");
            selenium.Type(popupPrefix + "AlertDate", DateTime.Today.AddDays(1).ToString("dd/MM/yyyy hh:mm"));
            selenium.Select("{0}AlertType_sfCombo".Formato(popupPrefix), "index=1");
            
            selenium.EntityOperationClick(AlertOperation.SaveNew);
            selenium.WaitAjaxFinished(() => !selenium.IsElementPresent(SeleniumExtensions.PopupSelector(popupPrefix)));
            
            selenium.WaitAjaxFinished(() => selenium.EntityHasNAlerts(1, future));
            selenium.WaitAjaxFinished(() => selenium.EntityHasNAlerts(0, warned));
            selenium.WaitAjaxFinished(() => selenium.EntityHasNAlerts(0, attended));

            selenium.GetAlert();

            //Create past
            selenium.AlertsCreateClick();

            selenium.WaitAjaxFinished(() => selenium.IsElementPresent(SeleniumExtensions.PopupSelector(popupPrefix)));

            selenium.Type(popupPrefix + "Text", "warned alert test");
            selenium.Select("{0}AlertType_sfCombo".Formato(popupPrefix), "index=1");
            selenium.Type(popupPrefix + "AlertDate", DateTime.Today.AddDays(-1).ToString("dd/MM/yyyy hh:mm"));
            
            selenium.EntityOperationClick(AlertOperation.SaveNew);
            selenium.WaitAjaxFinished(() => !selenium.IsElementPresent(SeleniumExtensions.PopupSelector(popupPrefix)));

            selenium.WaitAjaxFinished(() => selenium.EntityHasNAlerts(1, future));
            selenium.WaitAjaxFinished(() => selenium.EntityHasNAlerts(1, warned));
            selenium.WaitAjaxFinished(() => selenium.EntityHasNAlerts(0, attended));

            selenium.GetAlert();

            //View warned alert and attend it
            selenium.AlertsViewClick(warned);
            selenium.WaitAjaxFinished(() => selenium.IsElementPresent(SeleniumExtensions.PopupSelector(popupPrefix)));
            // Use "RowSelector a" => if only RowSelector the initial "No results" row will match
            selenium.WaitAjaxFinished(() => selenium.IsElementPresent("{0} a".Formato(SearchTestExtensions.RowSelector(selenium, 1, popupPrefix))));

            selenium.EntityClick(1, popupPrefix);
            selenium.WaitForPageToLoad(PageLoadTimeout);

            selenium.EntityOperationClick(AlertOperation.Attend);
            selenium.WaitAjaxFinished(() => selenium.EntityOperationEnabled(AlertOperation.Unattend));
            
            selenium.Open(viewRoute);
            selenium.WaitForPageToLoad(PageLoadTimeout);

            Assert.IsTrue(selenium.EntityHasNAlerts(0, warned));
            Assert.IsTrue(selenium.EntityHasNAlerts(1, future));
            Assert.IsTrue(selenium.EntityHasNAlerts(1, attended));
        }
    }
}
