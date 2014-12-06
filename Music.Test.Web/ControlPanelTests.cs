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
using Signum.Entities.Excel;
using Signum.Engine.Basics;
using Signum.Entities;
using Signum.Entities.DynamicQuery;
using Signum.Engine.DynamicQuery;
using Signum.Entities.UserQueries;
using Signum.Engine.UserQueries;
using Signum.Entities.Dashboard;
using Signum.Test.Environment;
using Signum.Entities.UserAssets;
using OpenQA.Selenium.Remote;

namespace Music.Test.Web
{
    [TestClass]
    public class DashboardTests : Common
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            Common.Start();

            AuthLogic.UnsafeUserSession("su").Using(_ =>
                new UserQueryEntity(typeof(AlbumEntity))
                {
                    Owner = Database.Query<UserEntity>().Where(u => u.UserName == "internal").Select(a => a.ToLite<Entity>()).SingleEx(),
                    DisplayName = "test",
                    Filters = { new QueryFilterEntity { Token = new QueryTokenEntity("Id"), Operation = FilterOperation.GreaterThan, ValueString = "3" } },
                }.ParseAndSave());
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            Common.MyTestCleanup();
        }

        [TestMethod]
        public void Dashboard001_Create()
        {
            Login();
            this.SearchPage(typeof(DashboardEntity))
                .Using(cps => cps.Create<DashboardEntity>())
                .EndUsing(dashboard =>
                {
                    dashboard.ValueLineValue(cp => cp.DisplayName, "Control Panel Home Page");
                    dashboard.ExecuteSubmit(DashboardOperation.Save);


                    string newPrefix;
                    PropertyRoute newRoute = dashboard.GetRoute(a=>a.Parts, out newPrefix);
                    var parts = new PartsRepeaterProxy(selenium, newPrefix, newRoute);

                    parts.CreatePartElement<UserQueryPartEntity>().Do(d =>
                    {
                        d.ValueLineValue(a => a.Title, "Last Albums");
                        d.EntityDetail(a => a.Content).Details<UserQueryPartEntity>().EntityLine(a => a.UserQuery).Find().SelectByPosition(0);
                    });

                    parts.CreatePartElement<CountSearchControlPartEntity>().Do(d =>
                    {
                        d.ValueLineValue(a => a.Title, "My count controls");
                        d.EntityDetail(a => a.Content).Details<CountSearchControlPartEntity>().EntityRepeater(a => a.UserQueries).Do(uq =>
                        {
                            uq.CreateElement<CountUserQueryElementEntity>().Do(cp => cp.EntityLine(a => a.UserQuery).Find().SelectByPosition(0));
                        });
                    });

                    parts.CreatePartElement<LinkListPartEntity>().Do(d =>
                    {
                        d.ValueLineValue(a => a.Title, "My Links");
                        d.EntityDetail(a => a.Content).Details<LinkListPartEntity>().EntityRepeater(a => a.Links).Do(le =>
                        {
                            le.CreateElement<LinkElementEntity>().Do(e =>
                            {
                                e.ValueLineValue(a => a.Label, "Best Band");
                                e.ValueLineValue(a => a.Link, "http://localhost/Music.Web/View/Band/1");
                            });


                            le.CreateElement<LinkElementEntity>().Do(e  =>
                            {
                                e.ValueLineValue(a => a.Label, "Best Artist");
                                e.ValueLineValue(a => a.Link, "http://localhost/Music.Web/View/Artist/1");
                            });
                        }); 
                    });

                    dashboard.ExecuteAjax(DashboardOperation.Save);

                    Assert.IsTrue(dashboard.HasId());

                }); 
        }
    }

    public class GridRepeaterProxy : EntityRepeaterProxy
    {
        public override int? NewIndex()
        {
            string result = (string)Selenium.ExecuteScript("window.$('#{0}_sfItemsContainer div.sf-grid-element').get().map(function(a){{return parseInt(a.id.substr('{0}'.length + 1));}}).join()".FormatWith(Prefix));

            return string.IsNullOrEmpty(result) ? 0 : result.Split(',').Select(int.Parse).Max() + 1;
        }

        public GridRepeaterProxy(RemoteWebDriver selenium, string prefix, PropertyRoute route)
            : base(selenium, prefix, route)
        {
        }
    }

    public class PartsRepeaterProxy : GridRepeaterProxy
    {
        public PartsRepeaterProxy(RemoteWebDriver selenium, string prefix, PropertyRoute route)
            : base(selenium, prefix, route)
        {
        }

        public LineContainer<PanelPartEntity> CreatePartElement<T>() where T : IPartEntity
        {
            var index = NewIndex();

            var prefix = "_".CombineIfNotEmpty(this.Prefix, index, "New");

            WaitChanges(() =>
            {
                Selenium.FindElement(CreateLocator).Click();

                new ChooserPopup(this.Selenium, prefix).EndUsing(e => e.Choose<T>());
            }, "create clicked");

            return this.Details<PanelPartEntity>(index.Value);
        }
    }
}
