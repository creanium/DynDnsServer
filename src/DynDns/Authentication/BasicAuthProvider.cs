using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using DynDns.Models.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using AuthenticationOptions = DynDns.Models.Options.AuthenticationOptions;

namespace DynDns.Authentication;

public class BasicAuthProvider(IOptionsMonitor<AuthenticationSchemeOptions> options, IOptionsMonitor<DynDnsServerOptions> serverOptions, ILoggerFactory loggerFactory, UrlEncoder encoder)
	: AuthenticationHandler<AuthenticationSchemeOptions>(options, loggerFactory, encoder)
{
	private readonly ILoggerFactory _loggerFactory = loggerFactory;
	internal const string SchemeName = "Basic";

	protected override Task<AuthenticateResult> HandleAuthenticateAsync()
	{
		var logger = _loggerFactory.CreateLogger<BasicAuthProvider>();

		var logins = serverOptions.CurrentValue.Authentication.Logins;
		logger.LogDebug("Handling Basic Authentication. Known users: {KnownUsers}", string.Join(", ", logins.Select(x => x.Username)));

		if (IsPublicEndpoint())
		{
			return Task.FromResult(AuthenticateResult.NoResult());
		}

		var authHeader = Request.Headers.Authorization.Count > 0 ? Request.Headers.Authorization[0] : null;

		logger.LogDebug("Authorization Header: {AuthorizationHeader}", authHeader);
		
		if (authHeader?.StartsWith(SchemeName) is true)
		{
			logger.LogInformation("Authorization Header present and matches {SchemeName}", SchemeName);
			
			var token = authHeader[SchemeName.Length..].Trim();
			var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(token)).Split(':');
			var username = credentials[0];
			var password = credentials[1];
			
			logger.LogDebug("Credentials supplied: {Credentials}", string.Join(" : ", credentials));
			
			var userMatch = logins
				.FirstOrDefault(x => x.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase) 
				                     && x.Password.Equals(password, StringComparison.InvariantCulture));
			
			if (userMatch is not null)
			{
				logger.LogInformation("User {Username} authenticated", userMatch.Username);
				
				var claims = new[]
				{
					new Claim("name", userMatch.Username),
					new Claim(ClaimTypes.Role, "Admin")
				};
				var identity = new ClaimsIdentity(claims, SchemeName);
				var principal = new ClaimsPrincipal(identity);
				var ticket = new AuthenticationTicket(principal, SchemeName);

				return Task.FromResult(AuthenticateResult.Success(ticket));
			}

			logger.LogWarning("User {Username} not authenticated", username);
			Response.StatusCode = 401;
			Response.Headers.Append("WWW-Authenticate", "Basic realm=\"DynDns\"");

			return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));
		}

		logger.LogWarning("{SchemeName} scheme not provided", SchemeName);
		Response.StatusCode = 401;
		Response.Headers.Append("WWW-Authenticate", "Basic realm=\"DynDns\"");

		return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));
	}

	bool IsPublicEndpoint()
		=> Context.GetEndpoint()?.Metadata.OfType<AllowAnonymousAttribute>().Any() is null or true;
}