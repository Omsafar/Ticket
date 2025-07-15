// App.xaml.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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

            // 1) EF Core
            services.AddDbContext<TicketContext>(options =>
        options.UseSqlServer("Server=192.168.1.24\\sgam;Database=PARATORI;User Id=sapara;Password=S@p4ra;Encrypt=True;TrustServerCertificate=True;")
        );

            // 2) Repository
            services.AddScoped<TicketRepository>();

            // 3) GraphMailReader
            services.AddSingleton(s => new GraphMailReader("support.ticket@paratorispa.it"));

            // 4) TicketManager
            services.AddSingleton<TicketManager>();

            // 5) Registra la MainWindow
            services.AddTransient<MainWindow>();

            _serviceProvider = services.BuildServiceProvider();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Autenticazione e inizializzazione Graph
            await GraphAuthProvider.InitializeAsync();

            // Avvia sincronizzazione in background
            var ticketManager = _serviceProvider.GetRequiredService<TicketManager>();
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    await ticketManager.SyncAsync(CancellationToken.None);
                    await Task.Delay(TimeSpan.FromMinutes(1));
                }
            });

            // Risolvi ed apri la finestra principale
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }
}
