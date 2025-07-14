using Microsoft.Identity.Client;
using Microsoft.Graph;
using Microsoft.Graph.Authentication;
using System.Threading.Tasks;

namespace TicketingApp.Graph
{
    public static class GraphAuthProvider
    {
        private const string CLIENT_ID = "<210dc5bc-cbb0-4cf5-bd62-7aea37c84608>";
        private const string TENANT_ID = "<419d9c9c-52c4-4cd1-8001-a350f5526c44>"; // directory (tenant) ID
        private static readonly string[] SCOPES =
        {
            "User.Read",              // info utente
            "Mail.Read.Shared"        // lettura mail caselle condivise
        };

        private static IPublicClientApplication _pca;
        private static InteractiveAuthenticationProvider _authProvider;

        public static GraphServiceClient GraphClient { get; private set; }

        public static async Task InitializeAsync()
        {
            _pca = PublicClientApplicationBuilder
                .Create(CLIENT_ID)
                .WithAuthority(AzureCloudInstance.AzurePublic, TENANT_ID)
                .WithDefaultRedirectUri()
                .Build();

            _authProvider = new InteractiveAuthenticationProvider(_pca, SCOPES);
            GraphClient = new GraphServiceClient(_authProvider);

            // Effettua un primo login per popolare il token cache
            await GraphClient.Me.Request().GetAsync();
        }
    }
}