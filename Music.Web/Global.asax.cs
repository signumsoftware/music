using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Signum.Test;
using System.Web.Hosting;
using Music.Web.Properties;
using Signum.Engine.Maps;
using Signum.Web.Operations;
using Signum.Entities.Authorization;
using System.Threading;
using Signum.Engine;
using Signum.Engine.Authorization;
using Signum.Entities;
using Signum.Web.UserQueries;
using Signum.Web.Excel;
using Signum.Web.Auth;
using Signum.Web.AuthAdmin;
using Signum.Web.Dashboard;
using Signum.Entities.Dashboard;
using Signum.Web.Combine;
using System.Reflection;
using Signum.Web.PortableAreas;
using Signum.Web.Chart;
using Signum.Utilities;
using Signum.Web.Files;
using Signum.Web.Processes;
using Signum.Web.Basic;
using Signum.Engine.Processes;
using Signum.Web.Notes;
using Signum.Web.Alerts;
using Signum.Web;
using Signum.Web.Exceptions;
using Signum.Test.Environment;
using System.IO;
using Signum.Web.DiffLog;

namespace Music.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
               Navigator.ViewRouteName,
               "View/{webTypeName}/{id}",
               new { controller = "Navigator", action = "View", webTypeName = "", id = "" }
            );

            routes.MapRoute(
               Navigator.CreateRouteName,
               "Create/{webTypeName}",
               new { controller = "Navigator", action = "Create", webTypeName = "" }
            );

            routes.MapRoute(
                Finder.FindRouteName,
                "Find/{webQueryName}",
                new { controller = "Finder", action = "Find", webQueryName = "" }
            );

            RouteTable.Routes.MapRoute(
                 "EmbeddedResources",
                 "{*file}",
                 new { controller = "Resources", action = "GetFile" },
                 new { file = new EmbeddedFileExist() }
            );
            
            routes.MapRoute(
                "Default",                                              // Route name
                "{controller}/{action}/{id}",                           // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional }  // Parameter defaults
            );
        }

        protected void Application_Start()
        {   
            Music.Test.Starter.Start(UserConnections.Replace(Settings.Default.ConnectionString));

            Statics.SessionFactory = new ScopeSessionFactory(new AspNetSessionFactory());

            using (AuthLogic.Disable())
            {
                Schema.Current.Initialize();
                WebStart();

                ProcessLogic.JustMyProcesses = true;
                ProcessRunnerLogic.MaxDegreeOfParallelism = 1;
                ProcessRunnerLogic.StartRunningProcesses(1000);
            }

            RegisterRoutes(RouteTable.Routes);
        }

        private void WebStart()
        {
            Navigator.Start(new NavigationManager("myLittleSecret"));
            Finder.Start(new FinderManager());
            Constructor.Start(new ConstructorManager(), new ClientConstructorManager());
            OperationClient.Start(new OperationManager(), true);

            AuthClient.Start(
                types: true, 
                property: true, 
                queries: true, 
                resetPassword: false,
                passwordExpiration: false);

            UserTicketClient.CookieName = "sfUserMusicSample";

            AuthAdminClient.Start(
                types: true, 
                properties: true, 
                queries: true, 
                operations: true, 
                permissions: true);

            QueryClient.Start();
            UserQueriesClient.Start();
            DashboardClient.Start();

            FilesClient.Start(true, true, true);
            ChartClient.Start();
            ExcelClient.Start(true, true);
            ExceptionClient.Start();
            DiffLogClient.Start();

            ProcessesClient.Start(packages: true, packageOperations: true);

            LinksClient.Start(widget: true, contextualItems: true);
            NoteClient.Start(typeof(LabelDN));
            AlertClient.Start(typeof(LabelDN));

            MusicClient.Start();

            ScriptHtmlHelper.Manager.MainAssembly = typeof(MusicClient).Assembly;
            SignumControllerFactory.MainAssembly = Assembly.GetExecutingAssembly();

            Navigator.Initialize();

            SignumControllerFactory.EveryController().AddFilters(
                new SignumExceptionHandlerAttribute());

            SignumControllerFactory.EveryController().AddFilters(
                ctx => ctx.FilterInfo.AuthorizationFilters.OfType<AuthenticationRequiredAttribute>().Any() ? null : new AuthenticationRequiredAttribute());
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            if (!UserTicketClient.LoginFromCookie())
                UserDN.Current = AuthLogic.AnonymousUser;
        }
    }
}
