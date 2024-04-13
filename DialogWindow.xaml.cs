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
using System.Windows.Shapes;

namespace AutoEvaluate
{
    public partial class DialogWindow : Window
    {
        private DialogWindow()
        {
            InitializeComponent();
        }

        public static void ShowMsg(Window owner, string msg, string btnContent = "确认")
        {
            var window = new DialogWindow() { Owner = owner};
            window.message.Text = msg;
            window.oneBtn.Visibility = Visibility.Visible;
            window.oneConfirmBtn.Content = btnContent;
            window.oneConfirmBtn.Click += (e, args) => {
                window.Close();
            };
            window.CloseBtn.MouseLeftButtonUp += (e, args) => {
                window.Close();
            };
            window.ShowDialog();
        }
        
        public static bool ShowTwoBtnMsg(Window owner, string msg, string confirmContent = "确认", string cancelContent = "取消", int lightBtn = 1)
        {
            var window = new DialogWindow() { Owner = owner};
            window.message.Text = msg;
            window.twoBtn.Visibility = Visibility.Visible;
            window.twoConfirmBtn.Content = confirmContent;
            window.twoCancelBtn.Content = cancelContent;
            if (lightBtn == 1)
            {
                window.twoConfirmBtn.Foreground = new SolidColorBrush(Colors.Snow);
                window.twoConfirmBtn.Background = new SolidColorBrush(Colors.DeepSkyBlue);
            }else if (lightBtn == 2)
            {
                window.twoCancelBtn.Foreground = new SolidColorBrush(Colors.Snow);
                window.twoCancelBtn.Background = new SolidColorBrush(Colors.DeepSkyBlue);
            }else if (lightBtn == 3)
            {
                window.twoConfirmBtn.Foreground = new SolidColorBrush(Colors.Snow);
                window.twoConfirmBtn.Background = new SolidColorBrush(Colors.DeepSkyBlue);
                window.twoCancelBtn.Foreground = new SolidColorBrush(Colors.Snow);
                window.twoCancelBtn.Background = new SolidColorBrush(Colors.DeepSkyBlue);
            }
            window.twoConfirmBtn.Click += (e, args) =>
            {
                window.DialogResult = true;
            };
            window.twoCancelBtn.Click += (e, args) =>
            {
                window.DialogResult = false;
            };
            window.CloseBtn.MouseLeftButtonUp += (e, args) =>
            {
                window.DialogResult = false;
            };
            return (bool) window.ShowDialog()!;
        }

        private void WindowDrag(object sender, MouseButtonEventArgs e)
        {
            if (e.Source != sender) return;
            DragMove();
        }
    }
}
