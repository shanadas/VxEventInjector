using Prism.Regions;
using StressAgent.ViewModels;
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

namespace StressAgent.Views
{
    /// <summary>
    /// Interaction logic for ShellCtrlView.xaml
    /// </summary>
    partial class ShellCtrlView : UserControl
    {
        public ShellCtrlView(ShellCtrlViewModel viewModel, IRegionManager regionMgr)
        {
            InitializeComponent();
            DataContext = viewModel;
            RegionManager.SetRegionManager(regionShellCtrl, regionMgr);
        }
    }
}
