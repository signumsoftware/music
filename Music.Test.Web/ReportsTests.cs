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
using Signum.Utilities;
using System.Resources;
using Signum.Entities.Reports;

namespace Music.Test.Web
{
    [TestClass]
    public class ReportsTests : Common
    {
        public ReportsTests()
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
        public void ExcelReport()
        {
            string pathAlbumSearch = FindRoute("Album");
            
            CheckLoginAndOpen(pathAlbumSearch);

            string excelMenuId = "tmExcel";
            string administerReportsId = "qbReportAdminister";

            string pathSampleReport = "D:\\Signum\\Pruebas\\Framework\\Albumchulo.xlsx";
            
            //create when there's no query created => direct navigation to create page
            selenium.QueryMenuOptionClick(excelMenuId, administerReportsId);
            selenium.WaitForPageToLoad(PageLoadTimeout);

            selenium.Type("DisplayName", "test");
            selenium.Type("File_sfFile", pathSampleReport);
            selenium.EntityOperationClick(ExcelReportOperation.Save);
            selenium.WaitForPageToLoad(SeleniumExtensions.PageLoadLongTimeout);
            selenium.MainEntityHasId();

            //modify
            selenium.Type("DisplayName", "test 2");
            selenium.EntityOperationClick(ExcelReportOperation.Save);
            selenium.WaitAjaxFinished(() => selenium.IsElementPresent("jq=.sf-entity-title:contains('test 2')"));

            //created appears modified in menu
            selenium.Open(pathAlbumSearch);
            selenium.WaitForPageToLoad(PageLoadTimeout);
            selenium.QueryMenuOptionPresentByAttr(excelMenuId, "title='test 2'", true);

            //delete
            selenium.QueryMenuOptionClick(excelMenuId, administerReportsId);
            selenium.WaitForPageToLoad(PageLoadTimeout);
            selenium.WaitAjaxFinished(() => selenium.IsElementPresent(SearchTestExtensions.RowSelector(selenium, 1))); //SearchOnLoad
            selenium.EntityClick("ExcelReport;1");
            selenium.WaitForPageToLoad(PageLoadTimeout);
            selenium.EntityOperationClick(ExcelReportOperation.Delete);
            Assert.IsTrue(Regex.IsMatch(selenium.GetConfirmation(), ".*"));
            selenium.WaitForPageToLoad(PageLoadTimeout);

            //deleted does not appear in menu
            selenium.Open(pathAlbumSearch);
            selenium.WaitForPageToLoad(PageLoadTimeout);
            selenium.QueryMenuOptionPresentByAttr(excelMenuId, "title='test 2'", false);

            //create when there are already others
            selenium.QueryMenuOptionClick(excelMenuId, administerReportsId);
            selenium.WaitForPageToLoad(PageLoadTimeout);
            selenium.SearchCreate();
            selenium.WaitForPageToLoad(PageLoadTimeout);
            selenium.Type("DisplayName", "test 3");
            selenium.Type("File_sfFile", pathSampleReport);
            selenium.EntityOperationClick(ExcelReportOperation.Save);
            selenium.WaitForPageToLoad(PageLoadTimeout);

            //created appears in menu
            selenium.Open(pathAlbumSearch);
            selenium.WaitForPageToLoad(PageLoadTimeout);
            selenium.QueryMenuOptionPresentByAttr(excelMenuId, "title='test 3'", true);
        }
    }
}
