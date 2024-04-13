using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AutoEvaluate.Utils;

namespace AutoEvaluate.Views
{
    public partial class LoginView : Page
    {
        public LoginView()
        {
            InitializeComponent();
        }

        public LoginView(string uname)
        {
            InitializeComponent();
            username.Text = uname;
        }

        private async void BtnClick(object sender, RoutedEventArgs e)
        {
            string un = username.Text, pw = password.Password;
            if (un == "")
            {
                DialogWindow.ShowMsg(Application.Current.MainWindow, "用户名不能为空");
                return;
            }
            if (pw == "")
            {
                DialogWindow.ShowMsg(Application.Current.MainWindow, "密码不能为空");
                return;
            }
            ((MainWindow)Application.Current.MainWindow).MainFrame.Navigate(new LoginLoadView());
            password.Password = "";
            password.Text = "";
            var res = await Evaluate.Login(un, pw);
            if (res.Item1)
            {
                var data = await Evaluate.GetEvaluationData();
                if (data == null)
                {
                    ((MainWindow)Application.Current.MainWindow).MainFrame.RemoveBackEntry();
                    ((MainWindow)Application.Current.MainWindow).MainFrame.Navigate(new LoginView(un));
                    DialogWindow.ShowMsg(Application.Current.MainWindow, "获取课程信息失败（登录成功）\n请重试或确认网络状态（最好使用校园网）以及教务系统是否正常。\n如无法解决请反馈。≧ ﹏ ≦");
                    return;
                }
                var storyboard = ((((MainWindow)Application.Current.MainWindow).FindName("CommonStates") as VisualStateGroup)!.States[1] as VisualState)!.Storyboard;
                storyboard.Completed += (e, args) =>
                {
                    VisualStateManager.GoToElementState(((MainWindow)Application.Current.MainWindow), "Normal", true);
                    ((MainWindow)Application.Current.MainWindow).MainFrame.RemoveBackEntry();
                    ((MainWindow)Application.Current.MainWindow).MainFrame.Navigate(new EvaluateView(data, un));
                    ((MainWindow)Application.Current.MainWindow).MainFrame.RemoveBackEntry();
                };
                VisualStateManager.GoToElementState((MainWindow)Application.Current.MainWindow, "Login", true);
                ((MainWindow)Application.Current.MainWindow).Left -= 150;
                ((MainWindow)Application.Current.MainWindow).Top -= 40;
            }
            else
            {
                ((MainWindow)Application.Current.MainWindow).MainFrame.RemoveBackEntry();
                ((MainWindow)Application.Current.MainWindow).MainFrame.Navigate(new LoginView(un));
                DialogWindow.ShowMsg(Application.Current.MainWindow, res.Item2);
            }
        }

        private void UserNamePreviewInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = new Regex("[^0-9]+").IsMatch(e.Text);
        }

        private void LoginWindowDrag(object sender, MouseButtonEventArgs e)
        {
            if (e.Source != sender) return;
            Application.Current.MainWindow.DragMove();
        }

        private void CloseBtn(object sender, MouseButtonEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }

        private void MinimizeBtn(object sender, MouseButtonEventArgs e)
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }
    }
}
