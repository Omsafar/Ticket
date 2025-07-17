using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TicketingApp.Data;
using TicketingApp.Models;
using TicketingApp.Services;

namespace TicketingApp
{
    public partial class ClosedTicketsWindow : Window
    {
        public ObservableCollection<string> States { get; } = new() { "Aperto", "In Corso", "In Pausa", "Chiuso" };
        public ObservableCollection<Ticket> Tickets { get; } = new();
        private readonly bool _isAdmin;
        private readonly string _currentEmail;
        private readonly TicketRepository _repo;
        private readonly TicketManager _manager;

        public ClosedTicketsWindow(TicketRepository repo, TicketManager manager, bool isAdmin, string currentEmail)
        {
            InitializeComponent();
            DataContext = this;
            _repo = repo;
            _manager = manager;
            _isAdmin = isAdmin;
            _currentEmail = currentEmail;
            TicketGrid.IsReadOnly = !_isAdmin;
            manager.TicketsSynced += () =>
            {
                Application.Current.Dispatcher.Invoke(LoadTickets);
            };
            LoadTickets();
        }

        private void LoadTickets()
        {
            Tickets.Clear();
            var query = _repo.GetAll().Where(t => EF.Functions.Like(t.Stato, "Chiuso"));
            if (!_isAdmin)
                query = query.Where(t => t.MittenteEmail == _currentEmail);
            foreach (var t in query.OrderByDescending(t => t.TicketId))
                Tickets.Add(t);
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadTickets();
        }

        private void TicketGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName == "GraphMessageId")
            {
                e.Cancel = true;
            }
            else if (e.PropertyName == "Stato")
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
                    IsReadOnly = !_isAdmin
                };
                e.Column = col;
            }
            else if (e.PropertyName == "Corpo" && e.Column is DataGridTextColumn textCol)
            {
                textCol.ElementStyle = new Style(typeof(TextBlock))
                {
                    Setters = { new Setter(TextBlock.TextWrappingProperty, TextWrapping.Wrap) }
                };
                textCol.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
            }
        }
        private async void TicketGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (!_isAdmin)
                return;

            if (e.Column.Header?.ToString() == "Stato" && e.EditAction == DataGridEditAction.Commit)
            {
                if (e.Row.Item is Ticket ticket)
                {
                    var oldStatus = await _repo.UpdateStatusAsync(ticket.TicketId, ticket.Stato);
                    _manager.NotifyTicketsChanged();
                    if (string.Equals(oldStatus, "Chiuso", System.StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(ticket.Stato, "Chiuso", System.StringComparison.OrdinalIgnoreCase))
                    {
                        await _manager.SendTicketReopenedNotificationAsync(ticket.MittenteEmail, ticket.TicketId);
                    }
                    _manager.NotifyTicketsChanged();
                }
            }
        }
    }
}