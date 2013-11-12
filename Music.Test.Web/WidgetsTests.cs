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
using Signum.Test.Environment;

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
            using (var label = NormalPage<LabelDN>(1, CheckLogin))
            {
                using (var albums = label.QuickLinkClickSearch(0))
                    Assert.IsTrue(albums.Results.RowsCount() > 0);
            }

            using (var label = SearchPage(typeof(LabelDN)))
            {
                label.Search();
                using (var albums = label.Results.EntityContextMenu(0).QuickLinkClickSearch(0))
                    Assert.IsTrue(albums.Results.RowsCount() > 0);
            }
        }

        [TestMethod]
        public void Widgets010_Notes()
        {
            using (var label = NormalPage<LabelDN>(1, CheckLogin))
            {
                Assert.AreEqual(0, label.NotesCount());

                using (var note = label.NotesCreateClick())
                {
                    note.ValueLineValue(a => a.Text, "note test");
                    note.ExecuteAjax(NoteOperation.Save);
                }

                selenium.Wait(() => selenium.IsAlertPresent());
                selenium.GetAlert();

                Assert.AreEqual(1, label.NotesCount());

                using (var notes = label.NotesViewClick())
                {
                    Assert.AreEqual(1, notes.Results.RowsCount());
                }
            }
        }

        [TestMethod]
        public void Widgets020_Alerts()
        {
            NormalPage<LabelDN>(1, CheckLogin).Using(label =>
            {
                Assert.AreEqual(0, label.AlertCount(AlertCurrentState.Attended));
                Assert.AreEqual(0, label.AlertCount(AlertCurrentState.Alerted));
                Assert.AreEqual(0, label.AlertCount(AlertCurrentState.Future));

                using (var alert = label.AlertCreateClick())
                {
                    alert.ValueLineValue(a => a.Text, "alert test");
                    alert.EntityCombo(a => a.AlertType).SelectIndex(0);
                    alert.ValueLineValue(a => a.AlertDate, DateTime.Today.AddDays(1));
                    alert.ExecuteAjax(AlertOperation.SaveNew);
                }

                selenium.Wait(() => selenium.IsAlertPresent());
                selenium.GetAlert();

                Assert.AreEqual(0, label.AlertCount(AlertCurrentState.Attended));
                Assert.AreEqual(0, label.AlertCount(AlertCurrentState.Alerted));
                Assert.AreEqual(1, label.AlertCount(AlertCurrentState.Future));

                using (var alert = label.AlertCreateClick())
                {
                    alert.ValueLineValue(a => a.Text, "warned alert test");
                    alert.EntityCombo(a => a.AlertType).SelectIndex(0);
                    alert.ValueLineValue(a => a.AlertDate, DateTime.Today.AddDays(-1));
                    alert.ExecuteAjax(AlertOperation.SaveNew);
                }

                selenium.Wait(() => selenium.IsAlertPresent());
                selenium.GetAlert();

                Assert.AreEqual(0, label.AlertCount(AlertCurrentState.Attended));
                Assert.AreEqual(1, label.AlertCount(AlertCurrentState.Alerted));
                Assert.AreEqual(1, label.AlertCount(AlertCurrentState.Future));

                using (var alerts = label.AlertsViewClick(AlertCurrentState.Alerted))
                {
                    return alerts.Results.EntityClick<AlertDN>(0);
                }
            })
            .Using(alert =>
            {
                alert.ExecuteAjax(AlertOperation.Attend);
                return NormalPage<LabelDN>(1);
            }).EndUsing(label =>
            {
                Assert.AreEqual(1, label.AlertCount(AlertCurrentState.Attended));
                Assert.AreEqual(0, label.AlertCount(AlertCurrentState.Alerted));
                Assert.AreEqual(1, label.AlertCount(AlertCurrentState.Future));
            });
        }
    }
}
