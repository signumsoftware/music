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
        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            Common.Start();

            AuthLogic.UnsafeUserSession("su").Using(_ =>
                new UserQueryDN(typeof(AlbumDN))
                {
                    Related = Database.Query<UserDN>().Where(u => u.UserName == "internal").Select(a => a.ToLite<IdentifiableEntity>()).SingleEx(),
                    DisplayName = "test",
                    Filters = { new QueryFilterDN { Token = new QueryTokenDN("Id"), Operation = FilterOperation.GreaterThan, Value = 3 } },
                }.ParseAndSave());
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            Common.MyTestCleanup();
        }

        [TestMethod]
        public void ControlPanel001_Create()
        {
            this.SearchPage(typeof(ControlPanelDN), CheckLogin)
                .Using(cps => cps.Create<ControlPanelDN>())
                .EndUsing(controlPanel =>
                {
                    controlPanel.ValueLineValue(cp => cp.DisplayName, "Control Panel Home Page");
                    controlPanel.ValueLineValue(a => a.NumberOfColumns, 2);
                    controlPanel.ExecuteSubmit(ControlPanelOperation.Save);

                    var parts = controlPanel.EntityRepeater(a => a.Parts);

                    controlPanel.CreateNewPart<UserQueryPartDN>(0).Do(d =>
                    {
                        d.ValueLineValue(a => a.Title, "Last Albums");
                        d.EntityLineDetail(a => a.Content).Details<UserQueryPartDN>().EntityLine(a => a.UserQuery).Find().SelectByPosition(0);
                    });

                    controlPanel.CreateNewPart<CountSearchControlPartDN>(1).Do(d =>
                    {
                        d.ValueLineValue(a => a.Title, "My count controls");
                        d.EntityLineDetail(a => a.Content).Details<CountSearchControlPartDN>().EntityRepeater(a => a.UserQueries).Do(uq =>
                        {
                            uq.Create();
                            uq.Details<CountUserQueryElement>(0).EntityLine(a => a.UserQuery).Find().SelectByPosition(0);
                        });
                    });

                    controlPanel.CreateNewPart<LinkListPartDN>(2).Do(d =>
                    {
                        d.ValueLineValue(a => a.Title, "My Links");
                        d.EntityLineDetail(a => a.Content).Details<LinkListPartDN>().EntityRepeater(a => a.Links).Do(le =>
                        {
                            le.Create();
                            le.Details<LinkElement>(0).ValueLineValue(a => a.Label, "Best Band");
                            le.Details<LinkElement>(0).ValueLineValue(a => a.Link, "http://localhost/Music.Web/View/Band/1");


                            le.Create();
                            le.Details<LinkElement>(1).ValueLineValue(a => a.Label, "Best Artist");
                            le.Details<LinkElement>(1).ValueLineValue(a => a.Link, "http://localhost/Music.Web/View/Artist/1");
                        }); 
                    });

                    controlPanel.Selenium.DragAndDropToObject(
                        "jq=#sfCpAdminContainer td[data-column=0] .sf-ftbl-part:eq(1)",
                        "jq=#sfCpAdminContainer td[data-column=1] .sf-ftbl-droppable");

                    controlPanel.ExecuteAjax(ControlPanelOperation.Save);

                    Assert.IsTrue(controlPanel.HasId());

                }); 
        }
    }
}
