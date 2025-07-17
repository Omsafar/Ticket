// App.xaml.cs/
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

            // 4) GraphMailSender
            services.AddSingleton<GraphMailSender>();

            // 5) TicketManager
            services.AddSingleton<TicketManager>();

            // 6) Registra la MainWindow
            services.AddTransient<MainWindow>();

            _serviceProvider = services.BuildServiceProvider();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Autenticazione e inizializzazione Graph
            await GraphAuthProvider.InitializeAsync();

            var ticketManager = _serviceProvider.GetRequiredService<TicketManager>();

            var currentEmail = GraphAuthProvider.CurrentUserEmail ?? string.Empty;
            ticketManager.CurrentUserEmail = currentEmail;
            ticketManager.IsAdmin = AdminSettings.Emails.Any(e => string.Equals(e, currentEmail, StringComparison.OrdinalIgnoreCase));
            ticketManager.CanSync = true;

            // Prima sincronizzazione bloccante all'avvio
            await ticketManager.SyncAsync(CancellationToken.None);

            // Risolvi ed apri la finestra principale
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            // La sincronizzazione sarà avviata manualmente dall'amministratore

        }
    }
}
