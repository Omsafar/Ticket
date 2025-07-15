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

        private readonly TicketRepository _repo;

        public MainWindow(TicketRepository repo, Services.TicketManager manager)
        {
            InitializeComponent();
            DataContext = this;

            _repo = repo;

            // Carica i ticket dal DB
            var all = _repo.GetAll().OrderByDescending(t => t.TicketId).ToList();
            foreach (var t in all)
                Tickets.Add(t);

            manager.TicketsSynced += () =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Tickets.Clear();
                    foreach (var t in _repo.GetAll().OrderByDescending(t => t.TicketId))
                        Tickets.Add(t);
                });
            };
        }
    }
}