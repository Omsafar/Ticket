using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using TicketingApp.Data;
using TicketingApp.Graph;
using TicketingApp.Services;

namespace TicketingApp
{
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider;

        public App()
        {
            var services = new ServiceCollection();

            // 1. EF Core
            services.AddDbContext<TicketContext>(options =>
                options.UseSqlServer("<CONNECTION_STRING_HERE>"));

            // 2. Repository
            services.AddScoped<TicketRepository>();

            // 3. Graph
            services.AddSingleton(s => new GraphMailReader("ticket@paratorispa.it"));

            // 4. TicketManager (background sync)
            services.AddSingleton<TicketManager>();

            _serviceProvider = services.BuildServiceProvider();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Inizializza Graph + login
            await GraphAuthProvider.InitializeAsync();

            // Avvia sync in background
            var ticketManager = _serviceProvider.GetRequiredService<TicketManager>();
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    await ticketManager.SyncAsync(System.Threading.CancellationToken.None);
                    await Task.Delay(TimeSpan.FromMinutes(1));
                }
            });

            // Mostra la finestra principale
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }
}