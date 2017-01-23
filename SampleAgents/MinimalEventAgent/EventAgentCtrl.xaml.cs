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

namespace MinimalEventAgent
{
    /// <summary>
    /// Interaction logic for EventAgentCtrl.xaml
    /// </summary>
    public partial class EventAgentCtrl : UserControl
    {
        public event EventHandler<string> NameModifiedEvent;

        public EventAgentCtrl()
        {
            InitializeComponent();
            DataContext = this;
        }

        public string CtrlName
        {
            set
            {
                FireNameModifiedEvent(value);
            }
        }

        private void FireNameModifiedEvent(string name)
        {
            var handler = NameModifiedEvent;
            if (handler != null)
                handler(this, name);
        }
    }
}
