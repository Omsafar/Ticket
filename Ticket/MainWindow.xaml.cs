using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using TicketingApp.Data;
using TicketingApp.Models;

namespace TicketingApp
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<Ticket> Tickets { get; } = new();

        public MainWindow(TicketRepository repo)
        {
            InitializeComponent();
            DataContext = this;

            // Carica i ticket dal DB
            var all = repo.GetAll().OrderByDescending(t => t.TicketId).ToList();
            foreach (var t in all)
                Tickets.Add(t);
        }
    }
}