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
        }

        private void LoadTickets()
        {
            Tickets.Clear();
            var query = _repo.GetAll();
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
                    SelectedItemBinding = new Binding("Stato") { Mode = BindingMode.TwoWay },
                    IsReadOnly = !_isAdmin
                };
                e.Column = col;
            }
            else if (e.PropertyName == "GraphMessageId")
            {
                e.Cancel = true;
            }
        }
    }
}
