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
using System.Text.RegularExpressions;
using Signum.Utilities;
using System.Resources;
using Signum.Entities.Reports;
using Signum.Entities;
using Signum.Test.Environment;

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
            string pathSampleReport = "D:\\Signum\\Music\\Assets\\Album.xlsx";

            SearchPage(typeof(AlbumDN), CheckLogin)
            .Using(albums =>
            {
                using (var report = albums.SearchControl.CreateExcelReport())
                {
                    report.ValueLineValue(a => a.DisplayName, "test");
                    report.FileLine(a => a.File).SetPath(pathSampleReport);
                    report.ExecuteAjax(ExcelReportOperation.Save);
                    Assert.IsTrue(report.HasId());

                    report.ValueLineValue(a => a.DisplayName, "test 2");
                    report.ExecuteAjax(ExcelReportOperation.Save);
                    selenium.WaitElementPresent("jq=.sf-entity-title:contains('test 2')");

                    return SearchPage(typeof(AlbumDN));
                }
            })
            .Using(albums =>
            {
                albums.Selenium.AssertElementPresent(albums.SearchControl.ExcelReportLocator("test 2"));

                using (var reports = albums.SearchControl.AdministerExcelReports())
                {
                    reports.SearchControl.WaitInitialSearchCompleted();
                    return reports.SearchControl.Results.EntityClick(Lite.Create<ExcelReportDN>(1));
                }
            })
            .Using(report => report.DeleteSubmit(ExcelReportOperation.Delete))
            .Using(reports => SearchPage(typeof(AlbumDN)))
            .Using(albums =>
            {
                albums.Selenium.AssertElementNotPresent(albums.SearchControl.ExcelReportLocator("title 2"));
                using (var report = albums.SearchControl.CreateExcelReport())
                {
                    report.ValueLineValue(a => a.DisplayName, "test 3");
                    report.FileLine(a => a.File).SetPath(pathSampleReport);
                    report.ExecuteAjax(ExcelReportOperation.Save);
                    return SearchPage(typeof(AlbumDN));
                }
            })
            .EndUsing(albums =>
            {
                albums.Selenium.AssertElementPresent(albums.SearchControl.ExcelReportLocator("test 3"));
            });
        }
    }
}
