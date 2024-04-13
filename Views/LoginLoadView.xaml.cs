using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AutoEvaluate.Views
{
    public partial class LoginLoadView : Page
    {
        public LoginLoadView()
        {
            InitializeComponent();
        }

        private void LoginWindowDrag(object sender, MouseButtonEventArgs e)
        {
            Application.Current.MainWindow.DragMove();
        }
    }
}
