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
using Signum.Windows.Excel;
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
using Signum.Windows.DiffLog;
using System.Windows.Controls;
using Signum.Utilities;

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
            Navigator.Start(new NavigationManager(multithreaded: true));
            Finder.Start(new FinderManager());
            Constructor.Start(new ConstructorManager());
            OperationClient.Start(new OperationManager());

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
            ExcelClient.Start(toExcel: true, excelReport: false);
            DiffLogClient.Start();

            UserQueryClient.Start();
            ChartClient.Start();

            Navigator.AddSettings(new List<EntitySettings>()
            {
                new EntitySettings<AlbumEntity>(){ View = e => new Album() },

                new EntitySettings<LabelEntity>() { View = e => new Music.Windows.Controls.Label() },
                new EntitySettings<ArtistEntity>() { View = e => new Artist() },
                new EntitySettings<BandEntity>() { View = e => new Band() },
                new EmbeddedEntitySettings<SongEntity>() { View = e => new Song() },

                new EntitySettings<AmericanMusicAwardEntity>() { View = e => new AmericanMusicAward() },
                new EntitySettings<GrammyAwardEntity>() { View = e => new GrammyAward() },
                new EntitySettings<PersonalAwardEntity>() { View = e => new PersonalAward() },

                new EntitySettings<CountryEntity>() { View = e => new Country() },

                new EntitySettings<NoteWithDateEntity>() { View = e => new NoteWithDate() },
            });

            Navigator.Initialize();
        }
    }
}
