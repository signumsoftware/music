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
using Signum.Entities.Chart;
using System.Text.RegularExpressions;
using Signum.Test.Environment;
using Signum.Entities.DynamicQuery;

namespace Music.Test.Web
{
    [TestClass]
    public class ChartTests : Common
    {
        public ChartTests()
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
        public void Chart001_Open_Filter_Draw_CreateUC()
        {
            string userChartName = "uctest" + DateTime.Now.Ticks.ToString();

            SearchPage(typeof(AlbumDN), CheckLogin)
            .Using(a =>
            {
                a.SearchControl.Filters.AddFilter("Id", FilterOperation.GreaterThan, 1);

                return a.SearchControl.OpenChart();
            })
            .Using(chart =>
            {
                //filter is maintained
                Assert.AreEqual("1", chart.Filters.GetFilter(0).ValueLine().StringValue);

                chart.GetColumnTokenBuilder(0).SelectToken("Author");
                chart.GetColumnTokenBuilder(1).SelectToken("Count");
                chart.Draw();

                selenium.Wait(() => chart.Results.RowsCount() == 3);
                chart.Filters.AddFilter("Count", FilterOperation.GreaterThan, 2);
                chart.Filters.QueryTokenBuilder.SelectToken("Id.Average");
                chart.Draw();

                selenium.Wait(() => chart.Results.RowsCount() == 2);
                chart.DataTab();
                chart.Results.OrderBy("Author");
                selenium.WaitElementPresent(chart.Results.RowLocator(0) + ":contains('Michael')");

                return chart.NewUserChart();
            })
            .Using(uc =>
            {
                uc.ValueLineValue(a => a.DisplayName, userChartName);

                selenium.AssertElementPresent(uc.EntityRepeater(a => a.Filters).RuntimeInfoLocator(0));
                selenium.AssertElementPresent(uc.EntityRepeater(a => a.Orders).RuntimeInfoLocator(0));

                uc.ExecuteSubmit(UserChartOperation.Save);

                return SearchPage(typeof(AlbumDN));
            })
            .Using(albums => albums.SearchControl.OpenChart())
            .Using(chart =>
            {
                chart.SelectUserChart(userChartName);

                chart.Draw();

                selenium.Wait(() => chart.Results.RowsCount() == 2);
                selenium.AssertElementPresent(chart.Results.RowLocator(0) + ":contains('Michael')");

                return chart.EditUserChart();
            })
            .Using(uc => uc.DeleteSubmit(UserChartOperation.Delete))
            .EndUsing(userCharts => userCharts.Selenium.AssertElementPresent(userCharts.SearchControl.SearchButtonLocator)); 
        }
    }
}
