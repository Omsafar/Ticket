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
        public ObservableCollection<Ticket> Tickets { get; } = new();
        private readonly bool _isAdmin;
        private readonly string _currentEmail;
        private readonly TicketRepository _repo;

        public ClosedTicketsWindow(TicketRepository repo, TicketManager manager, bool isAdmin, string currentEmail)
        {
            InitializeComponent();
            DataContext = this;
            _repo = repo;
            _isAdmin = isAdmin;
            _currentEmail = currentEmail;
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
            else if (e.PropertyName == "Corpo" && e.Column is DataGridTextColumn textCol)
            {
                textCol.ElementStyle = new Style(typeof(TextBlock))
                {
                    Setters = { new Setter(TextBlock.TextWrappingProperty, TextWrapping.Wrap) }
                };
                textCol.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
            }
        }
    }
}