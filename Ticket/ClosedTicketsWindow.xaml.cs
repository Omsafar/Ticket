using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TicketingApp.Data;
using TicketingApp.Models;
using TicketingApp.Services;
using System.Windows.Input;
using TicketingApp.Graph;
using System.Threading;

namespace TicketingApp
{
    public partial class ClosedTicketsWindow : Window
    {
        public ObservableCollection<Ticket> Tickets { get; } = new();
        private readonly bool _isAdmin;
        private readonly string _currentEmail;
        private readonly TicketManager _manager;
        private readonly TicketRepository _repo;

        public ClosedTicketsWindow(TicketRepository repo, TicketManager manager, bool isAdmin, string currentEmail)
        {
            InitializeComponent();
            DataContext = this;
            _repo = repo;
            _manager = manager;
            _isAdmin = isAdmin;
            _currentEmail = currentEmail;
            _manager.TicketsSynced += () =>
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

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isAdmin)
                await _manager.SyncAsync(System.Threading.CancellationToken.None);
            LoadTickets();
        }

        private void TicketGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName == "GraphMessageId" ||
                e.PropertyName == "GestoreEmail" ||
                (e.PropertyName == "ConversationId" && !_isAdmin))
            {
                e.Cancel = true;
            }
            else if (e.PropertyName == "Stato")
            {
                e.Column.DisplayIndex = 4;
                if (_isAdmin && e.Column is DataGridTextColumn statoCol)
                {
                    var style = new Style(typeof(DataGridCell));
                    style.Setters.Add(new Setter(DataGridCell.ContextMenuProperty, BuildUnlockMenu()));
                    style.Setters.Add(new EventSetter(DataGridCell.MouseLeftButtonUpEvent, new MouseButtonEventHandler(StatusCell_Click)));
                    statoCol.CellStyle = style;
                }
            }
            else if (e.PropertyName == "Corpo" && e.Column is DataGridTextColumn textCol)
            {
                textCol.ElementStyle = new Style(typeof(TextBlock))
                {
                    Setters = { new Setter(TextBlock.TextWrappingProperty, TextWrapping.Wrap) }
                };
                textCol.Width = DataGridLength.Auto;
            }
        }

        private ContextMenu BuildUnlockMenu()
        {
            var menu = new ContextMenu();
            var item = new MenuItem { Header = "Sblocca" };
            item.Click += UnlockMenuItem_Click;
            menu.Items.Add(item);
            return menu;
        }

        private void StatusCell_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is DataGridCell cell && cell.ContextMenu != null)
            {
                cell.ContextMenu.DataContext = cell.DataContext;
                cell.ContextMenu.IsOpen = true;
            }
        }

        private async void UnlockMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menu && menu.DataContext is Ticket ticket)
            {
                var result = MessageBox.Show($"Riaprire il ticket {ticket.TicketCode}?", "Conferma", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    await _repo.UpdateStatusAsync(ticket.TicketId, "Aperto");
                    var mailSender = new GraphMailSender();
                    await mailSender.SendTicketReopenedNotificationAsync("support.ticket@paratorispa.it", ticket.MittenteEmail, ticket.TicketId);
                    _manager.NotifyTicketsChanged();
                    LoadTickets();
                }
            }
        }

    }
}