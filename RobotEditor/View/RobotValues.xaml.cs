using System.Windows;

namespace RobotEditor.View
{
    /// <summary>
    /// Interaktionslogik für RobotValues.xaml
    /// </summary>
    public partial class RobotValues
    {
        public RobotValues()
        {
            InitializeComponent();
         }

        private void OK_Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

    }
}
