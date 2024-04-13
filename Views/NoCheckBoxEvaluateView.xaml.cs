using AutoEvaluate.Utils;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AutoEvaluate.Views
{
    public partial class NoCheckBoxEvaluateView : Page
    {
        List<EvaluationItem> EvaluationData;
        public NoCheckBoxEvaluateView(List<EvaluationItem> evaluationData, int type)
        {
            InitializeComponent();
            EvaluationData = evaluationData;
            listView.ItemsSource = EvaluationData;
            if (type == 1) TimeName.Text = "评教时间";
            else TimeName.Text = "截止时间";
        }

        public void AddItem(EvaluationItem item)
        {
            EvaluationData.Add(item);
            listView.Items.Refresh();
        }
    }
}
