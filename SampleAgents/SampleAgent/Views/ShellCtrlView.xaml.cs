﻿using SampleAgent.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SampleAgent.Views
{
    /// <summary>
    /// Interaction logic for ShellCtrlView.xaml
    /// </summary>
    partial class ShellCtrlView : UserControl
    {
        public ShellCtrlView(ShellCtrlViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
