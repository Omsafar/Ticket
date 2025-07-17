using System.Windows;

namespace TicketingApp
{
    public partial class NewTicketWindow : Window
    {
        public NewTicketWindow()
        {
            InitializeComponent();
        }

        public string MittenteEmail => EmailTextBox.Text;
        public string Oggetto => SubjectTextBox.Text;
        public string Corpo => BodyTextBox.Text;

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}