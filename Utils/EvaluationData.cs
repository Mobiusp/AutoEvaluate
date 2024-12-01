using System.ComponentModel;
using System.Windows;

namespace AutoEvaluate.Utils
{

    public class EvaluationData
    {
        public List<EvaluationItem> Unevaluated = [];
        public List<EvaluationItem> Evaluated = [];
        public List<EvaluationItem> UnexpiredOrNotStarted = [];
        public EvaluationData() { }
    }
    public class EvaluationItem : INotifyPropertyChanged
    {
        public string TeacherName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;

        public string GROUPNO { get; set; } = String.Empty;
        public string PJLXDM { get; set; } = String.Empty ;
        public int XUH {  get; set; } = 1;
        private bool isChecked = true;
        public bool IsChecked 
        { 
            get {  return isChecked; }
            set { 
                isChecked = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsChecked"));
            }
        }
        public string EvaluationTime { get; set; } = string.Empty;

        public EvaluationItem(string teacherName, string courseName, string gROUPNO, string pJLXDM, int xUH,string evaluationTime, bool isChecked = true)
        {
            TeacherName = teacherName;
            CourseName = courseName;
            GROUPNO = gROUPNO;
            PJLXDM = pJLXDM;
            XUH = xUH;
            IsChecked = isChecked;
            EvaluationTime = evaluationTime;
        }

        /*public EvaluationItem(string id, string teacherName, string courseName, string evaluationTime, string G, bool isChecked = true)
        {
            Id = id;
            TeacherName = teacherName;
            CourseName = courseName;
            EvaluationTime = evaluationTime;
            IsChecked = isChecked;
        }*/
        public EvaluationItem() { }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
