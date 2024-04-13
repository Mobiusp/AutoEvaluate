using AutoEvaluate.Utils;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;

namespace AutoEvaluate.Views
{
    public partial class EvaluateView : Page
    {
        private int nowPage = 0;
        public List<Page> pages = [];
        private readonly string loginUN;
        public bool isRunning = false;
        public EvaluateView(EvaluationData evaluationData, string un)
        {
            InitializeComponent();
            Frame.JournalOwnership = JournalOwnership.OwnsJournal;
            pages.Add(new NoCheckBoxEvaluateView(evaluationData.UnexpiredOrNotStarted, 2));
            pages.Add(new NoCheckBoxEvaluateView(evaluationData.Evaluated, 1));
            pages.Add(new Unevaluated(evaluationData.Unevaluated, this));
            loginUN = un;
        }

        private void Page_Load(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(pages[0]);
            Frame.LoadCompleted += AddPage;
        }

        public int AddIndex = 0;

        public void AddPage(object sender, NavigationEventArgs e)
        {
            AddIndex++;
            if (AddIndex == pages.Count)
            {
                Frame.LoadCompleted -= AddPage;
                return;
            }
            Frame.Navigate(pages[AddIndex]);
        }

        public void GithubBtn(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "https://github.com/Mobiusp/AutoEvaluate");
        }

        private void WindowDrag(object sender, MouseButtonEventArgs e)
        {
            if (e.Source != sender) return;
            Application.Current.MainWindow.DragMove();
        }
        private void CloseBtn(object sender, MouseButtonEventArgs e)
        {
            if (isRunning)
            {
                bool res = DialogWindow.ShowTwoBtnMsg(Application.Current.MainWindow, "当前正在评教中， 确认要关闭吗？", "关闭", "继续评教", 2);
                if (! res) return;
            }
            Application.Current.MainWindow.Close();
        }

        private void MinimizeBtn(object sender, MouseButtonEventArgs e)
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        private void PageBtn(object sender, MouseButtonEventArgs e)
        {
            Label pageBtn = (Label)sender;
            int index = int.Parse(pageBtn.Tag.ToString()!.Substring(4));

            unevaluated.Foreground = new SolidColorBrush(Colors.White);
            unevaluated.FontSize = 16;
            evaluated.Foreground = new SolidColorBrush(Colors.White);
            evaluated.FontSize = 16;
            unexpiredOrNotStarted.Foreground = new SolidColorBrush(Colors.White);
            unexpiredOrNotStarted.FontSize = 15;

            pageBtn.Foreground = new SolidColorBrush(Colors.Yellow);
            if (index != 2) pageBtn.FontSize = 17;
            else pageBtn.FontSize = 16;

            if (index > nowPage) for (int i = nowPage; i < index; ++i) Frame.GoBack();
            else if (index < nowPage) for (int i = index; i < nowPage; ++i) Frame.GoForward();
            nowPage = index;
        }
        
        private void LogoutBtn(object sender, MouseButtonEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).MainFrame.Navigate(new LoginLoadView());
            ((MainWindow)Application.Current.MainWindow).MainFrame.RemoveBackEntry();
            var storyboard = ((((MainWindow)Application.Current.MainWindow).FindName("CommonStates") as VisualStateGroup)!.States[2] as VisualState)!.Storyboard;
            storyboard.Completed += (e, args) =>
            {
                VisualStateManager.GoToElementState(((MainWindow)Application.Current.MainWindow), "Normal", true);
                ((MainWindow)Application.Current.MainWindow).MainFrame.Navigate(new LoginView(loginUN));
                ((MainWindow)Application.Current.MainWindow).MainFrame.RemoveBackEntry();
            };
            VisualStateManager.GoToElementState((MainWindow)Application.Current.MainWindow, "Logout", true);
            ((MainWindow)Application.Current.MainWindow).Left += 150;
            ((MainWindow)Application.Current.MainWindow).Top += 40;
        }
    }
}
