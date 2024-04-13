using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AutoEvaluate.Controls
{
    public class TextPasswordBox : TextBox
    {
        static TextPasswordBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TextPasswordBox), new FrameworkPropertyMetadata(typeof(TextBox)));
        }

        public string Password
        {
            get { return (string)GetValue(PasswordProperty); }
            set { SetValue(PasswordProperty, value); }
        }

        public char PasswordChar { get; set; } = '●';

        public static readonly DependencyProperty PasswordProperty = DependencyProperty.Register("Password", typeof(string),
            typeof(TextPasswordBox), new PropertyMetadata("", new PropertyChangedCallback(OnPasswordChanged)));

        private static void OnPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) { }

        protected override void OnTextInput(TextCompositionEventArgs e)
        {
            base.OnTextInput(e);
            var start = -1;
            for (int i = 0; i < Text.Length; ++i)
            {
                if (Text[i] != PasswordChar)
                {
                    start = i;
                    break;
                }
            }
            if (start == -1 && Text.Length == Password.Length) return;
            var end = -1;
            for (int i = Text.Length - 1; i >= 0; --i)
            {
                if (Text[i] != PasswordChar)
                {
                    end = i;
                    break;
                }
            }
            if (start != -1)
            {
                var change = end - start + 1;
                if (Text.Length == Password.Length) Password = Password.Substring(0, start) + Text.Substring(start, change) + Password.Substring(end + 1);
                else
                {
                    if (Text.Length - Password.Length < change) Password = Password.Substring(0, start) + Text.Substring(start, change) + Password.Substring(Password.Length - Text.Length + end + 1);
                    else Password = Password.Substring(0, start) + Text.Substring(start, change) + Password.Substring(start);
                }
            }
            else
            {
                if (SelectionStart < Text.Length) Password = Password.Substring(0, SelectionStart) + Password.Substring(SelectionStart + Password.Length - Text.Length);
                else Password = Password.Substring(0, SelectionStart);
            }
            var selectionStart = SelectionStart;
            Text = new string(PasswordChar, Text.Length);
            SelectionStart = selectionStart;
        }
    }
}
