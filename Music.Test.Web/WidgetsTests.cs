using System;
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
            Login(); 

            using (var label = NormalPage<LabelEntity>(1))
            {
                using (var albums = label.QuickLinkClickSearch("Album"))
                    Assert.IsTrue(albums.Results.RowsCount() > 0);
            }

            using (var label = SearchPage(typeof(LabelEntity)))
            {
                label.Search();
                using (var albums = label.Results.EntityContextMenu(0).QuickLinkClickSearch("Album"))
                {
                    selenium.Wait(() => albums.Results.RowsCount() > 0);
                }
                    
            }
        }

        [TestMethod]
        public void Widgets010_Notes()
        {
            Login();
            using (var label = NormalPage<LabelEntity>(1))
            {
                Assert.AreEqual(0, label.NotesCount());

                using (var note = label.NotesCreateClick())
                {
                    note.ValueLineValue(a => a.Text, "note test");
                    note.ExecuteAjax(NoteOperation.Save);
                }

                selenium.Wait(() => 1 == label.NotesCount());

                using (var notes = label.NotesViewClick())
                {
                    Assert.AreEqual(1, notes.Results.RowsCount());
                }
            }
        }

        [TestMethod]
        public void Widgets020_Alerts()
        {
            Login();
            NormalPage<LabelEntity>(1).Using(label =>
            {

                Assert.IsTrue(label.AlertsAre(0, 0, 0));

                using (var alert = label.AlertCreateClick())
                {
                    alert.ValueLineValue(a => a.Text, "alert test");
                    alert.EntityCombo(a => a.AlertType).SelectIndex(0);
                    alert.ValueLineValue(a => a.AlertDate, DateTime.Today.AddDays(1));
                    alert.ExecuteAjax(AlertOperation.SaveNew);
                }

                selenium.Wait(()=>label.AlertsAre(0, 0, 1));

                using (var alert = label.AlertCreateClick())
                {
                    alert.ValueLineValue(a => a.Text, "warned alert test");
                    alert.EntityCombo(a => a.AlertType).SelectIndex(0);
                    alert.ValueLineValue(a => a.AlertDate, DateTime.Today.AddDays(-1));
                    alert.ExecuteAjax(AlertOperation.SaveNew);
                }

                selenium.Wait(() => label.AlertsAre(0, 1, 1));

                using (var alerts = label.AlertsViewClick(AlertCurrentState.Alerted))
                {
                    return alerts.Results.EntityClick<AlertEntity>(0);
                }
            })
            .Using(alert =>
            {
                alert.ExecuteAjax(AlertOperation.Attend);
                return NormalPage<LabelEntity>(1);
            }).EndUsing(label =>
            {

                selenium.Wait(() => label.AlertsAre(1, 0, 1));
            });
        }
    }
}
