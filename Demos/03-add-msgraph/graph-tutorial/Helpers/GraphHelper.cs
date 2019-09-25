// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace graph_tutorial.Helpers
{
	using graph_tutorial.TokenStorage;
	using Microsoft.Graph;
	using Microsoft.Identity.Client;
	using System.Collections.Generic;
	using System.Configuration;
	using System.Linq;
	using System.Net.Http.Headers;
	using System.Security.Claims;
	using System.Threading.Tasks;
	using System.Web;
	using static System.Globalization.CultureInfo;


	public static class GraphHelper
	{
		// Load configuration settings from PrivateSettings.config
		private static readonly string appId = ConfigurationManager.AppSettings["ida:AppId"];
		private static readonly string appSecret = ConfigurationManager.AppSettings["ida:AppSecret"];
		private static readonly string tenantId = ConfigurationManager.AppSettings["ida:TenantID"];
		private static readonly string redirectUri = ConfigurationManager.AppSettings["ida:RedirectUri"];
		private static readonly string graphScopes = ConfigurationManager.AppSettings["ida:AppScopes"];

		public static async Task<User> GetUserDetailsAsync(string accessToken)
		{
			var graphClient = new GraphServiceClient(
				new DelegateAuthenticationProvider(
						async (requestMessage) =>
						{
							requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
						}));

			return await graphClient.Me.Request().GetAsync();
		}

		public static async Task<IEnumerable<Event>> GetEventsAsync()
		{
			var graphClient = GetAuthenticatedClient();
			var events = await graphClient.Me.Events.Request().Select("subject,organizer,start,end").OrderBy("createdDateTime DESC").GetAsync();
			return events.CurrentPage;
		}

		private static GraphServiceClient GetAuthenticatedClient()
		{
			return new GraphServiceClient(
				new DelegateAuthenticationProvider(
					async (requestMessage) =>
					{
						var idClient = ConfidentialClientApplicationBuilder.Create(appId)
							.WithAdfsAuthority(authorityUri: string.Format(InvariantCulture, "https://login.microsoftonline.com/{0}", tenantId))
							.WithRedirectUri(redirectUri)
							.WithClientSecret(appSecret)
							.Build();
						var tokenStore = new SessionTokenStore(idClient.UserTokenCache, HttpContext.Current, ClaimsPrincipal.Current);
						var accounts = await idClient.GetAccountsAsync();
						// By calling this here, the token can be refreshed if it's expired right before the Graph call is made
						var scopes = graphScopes.Split(' ');
						var result = await idClient.AcquireTokenSilent(scopes, accounts.FirstOrDefault()).ExecuteAsync();
						requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
					}));
		}
	}
}
