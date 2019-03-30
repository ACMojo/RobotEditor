using System.Windows;

namespace RobotEditor.View
{
    /// <summary>
    ///     Interaktionslogik für RobotValues.xaml
    /// </summary>
    public partial class RobotValues
    {
        #region Instance

        public RobotValues()
        {
            InitializeComponent();
        }

        #endregion

        #region Private methods

        private void OK_Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        #endregion
    }
}