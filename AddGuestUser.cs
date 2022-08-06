using Azure.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Graph;

namespace GuestUserGraphFx;

public class AddGuestUser
{
    [Function(nameof(AddUser))]
    public async Task<HttpResponseData> AddUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")]
        HttpRequestData request)
    {
        var queryDictionary = QueryHelpers.ParseQuery(request.Url.Query);
        var firstName = queryDictionary["firstName"];
        var email = queryDictionary["email"];

        var invitation = new Invitation
        {
            InvitedUserDisplayName = $"{firstName}",
            InvitedUserEmailAddress = email,
            InviteRedirectUrl = "https://myapplications.microsoft.com/",
            InvitedUserMessageInfo = new()
            {
                CustomizedMessageBody =
                    "Welcome to my organization. Please request access to applications through the self service portal."
            },
            SendInvitationMessage = true,
            Id = email
        };

        var graphClient = GetAuthenticatedGraphClient();
        var graphResult = await graphClient.Invitations
            .Request()
            .AddAsync(invitation);

        if (graphResult != null)
        {
            return await request.Ok("ok");
        }

        return await request.BadRequestAsync("error");
    }

    private static GraphServiceClient GetAuthenticatedGraphClient()
    {
        // Read more about scopes: https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-permissions-and-consent
        var scopes = new[] { "https://graph.microsoft.com/.default" };

        // Values from app registration
        var tenantId = Environment.GetEnvironmentVariable("AzureADTenantId", EnvironmentVariableTarget.Process);
        var clientId = Environment.GetEnvironmentVariable("AzureADAppId", EnvironmentVariableTarget.Process);
        var clientSecret = Environment.GetEnvironmentVariable("AzureADAppSecret", EnvironmentVariableTarget.Process);

        var options = new TokenCredentialOptions
        {
            AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
        };

        var clientSecretCredential = new ClientSecretCredential(
            tenantId, clientId, clientSecret, options);

        return new(clientSecretCredential, scopes);
    }
}