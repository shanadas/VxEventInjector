﻿using System;
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
using VxEventInjector.ViewModels;

namespace VxEventInjector.Views
{
    /// <summary>
    /// Interaction logic for VxLogin.xaml
    /// </summary>
    partial class VxLoginView : UserControl
    {
        public VxLoginView(VxLoginViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}