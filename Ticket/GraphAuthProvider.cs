// GraphAuthProvider.cs
using Azure.Identity;
using Microsoft.Graph;
using System.Threading.Tasks;

namespace TicketingApp.Graph
{
    public static class GraphAuthProvider
    {
        private const string CLIENT_ID = "210dc5bc-cbb0-4cf5-bd62-7aea37c84608";
        private const string TENANT_ID = "419d9c9c-52c4-4cd1-8001-a350f5526c44";

        // Gli scope *delegated* che vogliamo
        private static readonly string[] SCOPES = { "User.Read", "Mail.Read.Shared" };

        public static GraphServiceClient GraphClient { get; private set; }

        public static async Task InitializeAsync()
        {
            // apre il browser di sistema per il login (SSO M365)
            var credential = new InteractiveBrowserCredential(
                new InteractiveBrowserCredentialOptions
                {
                    TenantId = TENANT_ID,
                    ClientId = CLIENT_ID
                });

            GraphClient = new GraphServiceClient(credential, SCOPES);

            // chiamata di test per forzare il token
            await GraphClient.Me.GetAsync();
        }
    }
}
