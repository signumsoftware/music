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
using Signum.Entities;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;

namespace Music.Test.Web
{
    [TestClass]
    [DeploymentItem("chromedriver.exe", "")]
    public class Common : SeleniumTestClass
    {
        protected override string Url(string url)
        {
            return "http://localhost/Music.Web/" + url;
        }

        public static void Start()
        {
            Music.Test.Starter.StartAndLoad(UserConnections.Replace(Settings.Default.ConnectionString));

            AuthLogic.GloballyEnabled = false;
            Schema.Current.Initialize();

            selenium = new ChromeDriver(); //new FirefoxDriver();
        }

        protected void Login()
        {
            Login("internal", "internal");
        }

        protected static void MyTestCleanup()
        {
            selenium.Close();
        }
    }
}
