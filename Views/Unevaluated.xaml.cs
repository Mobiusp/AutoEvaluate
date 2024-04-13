using AutoEvaluate.Utils;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

namespace AutoEvaluate.Views
{
    public partial class Unevaluated : Page
    {
        private int sum = 0;
        private int checkedCnt = 0;
        private List<EvaluationItem> evaluationItems;
        private EvaluateView Owner;
        private int FailedCnt = 0;
        private int FinishedCnt = 0;
        private int needCnt = 0;
        private bool BtnDisable = false;

        public Unevaluated(List<EvaluationItem> evaluationItem, EvaluateView owner)
        {
            InitializeComponent();
            Owner = owner;
            evaluationItems = evaluationItem;
            listView.ItemsSource = evaluationItems;
            sum = evaluationItems.Count ;
            checkedCnt = evaluationItems.Count;
            checkAll.IsChecked = true;
        }

        private void CheckBoxClick(object sender, RoutedEventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            if (cb.IsChecked ?? false)
            {
                if (cb.Tag.ToString() == "checkAll")
                {
                    foreach (EvaluationItem item in evaluationItems) item.IsChecked = true;
                    checkedCnt = sum;
                }
                else
                {
                    ++checkedCnt;
                    if (checkedCnt == sum) checkAll.IsChecked = true;
                }
            }
            else
            {
                if (cb.Tag.ToString() == "checkAll")
                {
                    foreach (EvaluationItem item in evaluationItems) item.IsChecked = false;
                    checkedCnt = 0;
                }
                else
                {
                    if (checkedCnt == sum) checkAll.IsChecked = false;
                    --checkedCnt;
                }
            }
        }

        private async void StartBtn(object sender, RoutedEventArgs e)
        {
            if (BtnDisable) { return; }
            BtnDisable = true;
            Owner.isRunning = true;
            ProcessBar.Visibility = Visibility.Visible;
            Bar.Width = 0;
            ProcessBarNum.Text = "  0%";
            List<EvaluationItem> needEvaluate = new List<EvaluationItem>();
            foreach (EvaluationItem item in evaluationItems)
            {
                if (item.IsChecked) needEvaluate.Add(item);
            }
            FailedCnt = 0;
            needCnt = needEvaluate.Count;
            FinishedCnt = 0;
            await Evaluate.InitEvaluateClient();
            foreach (EvaluationItem item in needEvaluate) 
            {
                bool result = await Evaluate.EvaluateCourse(item.Id);
                if (result)
                {
                    ListViewItem t = (ListViewItem)listView.ItemContainerGenerator.ContainerFromItem(evaluationItems[0]);
                    var container = VisualTreeHelper.GetChild((VisualTreeHelper.GetChild(t, 0) as Border)!.Child, 0);
                    var storyborad = (((container as Grid)!.FindName("CommonStates") as VisualStateGroup)!.States[1] as VisualState)!.Storyboard;
                    storyborad.Completed += (s, args) =>
                    {
                        (Owner.pages[1] as NoCheckBoxEvaluateView)!.AddItem(item);
                        evaluationItems.Remove(item);
                        listView.Items.Refresh();
                    };
                    VisualStateManager.GoToElementState(container as Grid, "Removed", true);
                }
                else ++FailedCnt;
                ++FinishedCnt;
                ProcessBarNum.Text = string.Format("{0,3}", (int)((double)FinishedCnt / needCnt * 100)) + "%";
                Bar.Width = ((double)FinishedCnt / needCnt) * 400;
            }
            string message = $"评教完成\n需评教课程数：{needCnt}\n评教成功：{needCnt - FailedCnt} 评教失败：{FailedCnt}";
            if (FailedCnt > 0) message += "失败原因可能为：\n1.该课程评教已截止\n2.该课程在脚本启动过程中已被评教\n3.网络状态较差或教务系统出错\n4.脚本出现错误";
            DialogWindow.ShowMsg(Application.Current.MainWindow, message);
            BtnDisable = false;
            Owner.isRunning = false;
        }
    }
}
