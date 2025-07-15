using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TicketingApp.Data
{
    // Questo viene usato da `dotnet ef` per creare il context
    // senza toccare la tua WPF App.xaml.cs
    public class TicketContextFactory : IDesignTimeDbContextFactory<TicketContext>
    {
        public TicketContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<TicketContext>();

            // Stessa connection string che usi in App.xaml.cs
            builder.UseSqlServer(
                "Server=192.168.1.24\\sgam;Database=PARATORI;User Id=sapara;Password=S@p4ra;Encrypt=True;TrustServerCertificate=True;"
            );

            return new TicketContext(builder.Options);
        }
    }
}
