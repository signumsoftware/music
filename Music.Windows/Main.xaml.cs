using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Signum.Entities;
using Signum.Utilities;
using Signum.Windows;
using Signum.Windows.Authorization;

namespace Music.Windows
{
    /// <summary>
    /// Interaction logic for Main.xaml
    /// </summary>
    public partial class Main : Window
    {
        public Main()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(Main_Loaded);
        }

        void Main_Loaded(object sender, RoutedEventArgs e)
        {
            //this.MaxHeight = this.ActualHeight;
            MenuManager.ProcessMenu(menu);
        }
   
        private void miUpdateRules_Click(object sender, RoutedEventArgs e)
        {
            AuthClient.UpdateCache();
        }
    }
}
