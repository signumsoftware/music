﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Signum.Windows;
using Signum.Entities;
using Signum.Test;

namespace Music.Windows.Controls
{
    /// <summary>
    /// Interaction logic for Song.xaml
    /// </summary>
    public partial class Song : UserControl
    {
        public Song(PropertyRoute route)
        {
            Common.SetPropertyRoute(this, route);
            InitializeComponent();
        }

        public Song()
        {
            Common.SetDelayedRoutes(this, true);
            InitializeComponent();
        }
    }
}
