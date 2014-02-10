using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Signum.Windows;
using Signum.Windows.Authorization;
using Signum.Windows.Operations;
using Signum.Windows.Processes;
using Signum.Windows.Reports;
using Signum.Windows.Scheduler;
using Signum.Entities;
using Signum.Entities.Basics;
using Signum.Windows.Files;
using System.Threading;
using Signum.Test;
using Music.Windows.Controls;
using Signum.Entities.DynamicQuery;
using Signum.Windows.Chart;
using Signum.Windows.UserQueries;
using Signum.Test.Environment;

namespace Music.Windows
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
            : base()
        {
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

            this.DispatcherUnhandledException += (object sender, DispatcherUnhandledExceptionEventArgs e) =>
            {
                Program.HandleException("Error inesperado", e.Exception, App.Current.MainWindow);
                e.Handled = true;
            };
            Async.DispatcherUnhandledException += (ex, win) => Program.HandleException("Error inesperado", ex, win);
            Async.AsyncUnhandledException += (ex, win) => Program.HandleException("Error inesperado", ex, win);

            InitializeComponent();
        }

        internal static BitmapSource LoadIcon(string segurosImage)
        {
            return ImageLoader.LoadIcon(PackUriHelper.Reference("Imagenes/" + segurosImage, typeof(App)));
        }

        public static void StartApplication()
        {
            Navigator.Start(new NavigationManager(multithreaded: true)
            {
                EntitySettings = new Dictionary<Type, EntitySettings>()
                {
                }
            });

            Constructor.Start(new ConstructorManager
            {
                Constructors = new Dictionary<Type, Func<FrameworkElement, object>>()
                {
              
                }
            });

            OperationClient.Start(new OperationManager
            {
                Settings = new Dictionary<Enum, OperationSettings>()
                {
                }
            });

            AuthClient.Start(
                types: true,
                property: true, 
                queries: true, 
                permissions: true, 
                operations: true, 
                defaultPasswordExpiresLogic: false);

            //ProcessClient.Start();
            //SchedulerClient.Start();

            LinksClient.Start(widget: true, contextualMenu: true);
            ReportClient.Start(toExcel: true, excelReport: false);

            UserQueryClient.Start();
            ChartClient.Start();

            Navigator.AddSettings(new List<EntitySettings>()
            {
                new EntitySettings<AlbumDN>(){ View = e => new Album() },

                new EntitySettings<LabelDN>() { View = e => new Label() },
                new EntitySettings<ArtistDN>() { View = e => new Artist() },
                new EntitySettings<BandDN>() { View = e => new Band() },
                new EmbeddedEntitySettings<SongDN>() { View = (e, pr) => new Song(pr) },

                new EntitySettings<AmericanMusicAwardDN>() { View = e => new AmericanMusicAward() },
                new EntitySettings<GrammyAwardDN>() { View = e => new GrammyAward() },
                new EntitySettings<PersonalAwardDN>() { View = e => new PersonalAward() },

                new EntitySettings<CountryDN>() { View = e => new Country() },

                new EntitySettings<NoteWithDateDN>() { View = e => new NoteWithDate() },
            });

            Navigator.Initialize();
        }
    }
}
