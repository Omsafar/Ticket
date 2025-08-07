using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using TicketingApp.Data;
using TicketingApp.Models;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using TicketingApp.Graph;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using TicketingApp.Services;
using MaterialDesignThemes.Wpf;


namespace TicketingApp
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<string> States { get; } = new() { "Aperto", "In Corso", "In Pausa", "Chiuso" };
        public ObservableCollection<Ticket> Tickets { get; } = new();
        private readonly bool _isAdmin;
        private readonly string _currentEmail;
        private readonly TicketRepository _repo;
        private readonly Services.TicketManager _manager;
        private readonly PaletteHelper _paletteHelper = new();
        private bool _isDarkTheme;

        public MainWindow(TicketRepository repo, Services.TicketManager manager)
        {
            InitializeComponent();
            DataContext = this;

            _repo = repo;
            _manager = manager;

            _currentEmail = GraphAuthProvider.CurrentUserEmail ?? string.Empty;

            _isAdmin = AdminSettings.Emails.Any(e => string.Equals(e, _currentEmail, StringComparison.OrdinalIgnoreCase));
            LoadTickets();

            _manager.TicketsSynced += () =>
            {
                Application.Current.Dispatcher.Invoke(LoadTickets);
            };

            SyncButton.Visibility = _isAdmin ? Visibility.Visible : Visibility.Collapsed;
            ClosedTicketsButton.Content = _isAdmin ? "Ticket chiusi" : "I miei ticket chiusi";
            NewTicketButton.Visibility = _isAdmin ? Visibility.Visible : Visibility.Collapsed;
        }

        private void LoadTickets()
        {
            Tickets.Clear();
            var query = _repo.GetAll().Where(t => !EF.Functions.Like(t.Stato, "Chiuso"));
            if (!_isAdmin)
                query = query.Where(t => t.MittenteEmail == _currentEmail);
            foreach (var t in query.OrderByDescending(t => t.TicketId))
                Tickets.Add(t);
        }

        private async void TicketGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (!_isAdmin)
                return;

            if (e.Column.Header?.ToString() == "Stato" && e.EditAction == DataGridEditAction.Commit)
            {
                if (e.Row.Item is Ticket ticket)
                {
                    await _repo.UpdateStatusAsync(ticket.TicketId, ticket.Stato);
                    _manager.NotifyTicketsChanged();
                }
            }
        }

        private void TicketGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName == "Stato")
            {
                var col = new DataGridComboBoxColumn
                {
                    Header = "Stato",
                    ItemsSource = States,
                    SelectedItemBinding = new Binding("Stato")
                    {
                        Mode = BindingMode.TwoWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    },
                    IsReadOnly = !_isAdmin,
                    DisplayIndex = 4
                };
                e.Column = col;
            }
            else if (e.PropertyName == "GraphMessageId" ||
                     e.PropertyName == "GestoreEmail" ||
                     (e.PropertyName == "ConversationId" && !_isAdmin))
            {
                e.Cancel = true;
            }
            else if (e.PropertyName == "Corpo" && e.Column is DataGridTextColumn textCol)
            {
                textCol.ElementStyle = new Style(typeof(TextBlock))
                {
                    Setters = { new Setter(TextBlock.TextWrappingProperty, TextWrapping.Wrap) }
                };
                textCol.IsReadOnly = true;
                textCol.Width = DataGridLength.Auto;
            }
            else
            {
                e.Column.IsReadOnly = true;
            }
        }
        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isAdmin)
                await _manager.SyncAsync(CancellationToken.None);
            LoadTickets();
        }

        private async void SyncButton_Click(object sender, RoutedEventArgs e)
        {
            await _manager.SyncAsync(CancellationToken.None);
        }
        private async void NewTicketButton_Click(object sender, RoutedEventArgs e)
        {
            var win = new NewTicketWindow();
            if (win.ShowDialog() == true)
            {
                await _manager.CreateManualTicketAsync(
                    win.MittenteEmail,
                    win.Oggetto,
                    win.Corpo);
                LoadTickets();
            }
        }
        private void ClosedTicketsButton_Click(object sender, RoutedEventArgs e)
        {
            var win = new ClosedTicketsWindow(_repo, _manager, _isAdmin, _currentEmail);
            win.Show();
        }


        private void ThemeToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            SetBaseTheme(true);
        }

        private void ThemeToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            SetBaseTheme(false);
        }

        private void SetBaseTheme(bool isDark)
        {
            _isDarkTheme = isDark;
            ITheme theme = _paletteHelper.GetTheme();
            theme.SetBaseTheme(isDark ? Theme.Dark : Theme.Light);
            _paletteHelper.SetTheme(theme);
        }
    }
}
