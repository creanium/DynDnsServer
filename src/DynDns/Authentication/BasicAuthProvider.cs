using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace DynDns.Authentication;

public class BasicAuthProvider(IOptionsMonitor<AuthenticationSchemeOptions> options,
	ILoggerFactory loggerFactory,
	UrlEncoder encoder)
	: AuthenticationHandler<AuthenticationSchemeOptions>(options, loggerFactory, encoder)
{
	internal const string SchemeName = "Basic";

	protected override Task<AuthenticateResult> HandleAuthenticateAsync()
	{
		var logger = loggerFactory.CreateLogger<BasicAuthProvider>();
		
		logger.LogDebug("Handling Basic Authentication");

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
			
			logger.LogDebug("Credentials supplied: {Credentials}", string.Join(" : ", credentials));
			
			if (credentials[0] == "admin" && credentials[1] == "password")
			{
				var claims = new[]
				{
					new Claim("name", credentials[0]),
					new Claim(ClaimTypes.Role, "Admin")
				};
				var identity = new ClaimsIdentity(claims, SchemeName);
				var principal = new ClaimsPrincipal(identity);
				var ticket = new AuthenticationTicket(principal, SchemeName);

				return Task.FromResult(AuthenticateResult.Success(ticket));
			}

			Response.StatusCode = 401;
			Response.Headers.Append("WWW-Authenticate", "Basic realm=\"DynDns\"");

			return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));
		}

		Response.StatusCode = 401;
		Response.Headers.Append("WWW-Authenticate", "Basic realm=\"DynDns\"");

		return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));
	}

	bool IsPublicEndpoint()
		=> Context.GetEndpoint()?.Metadata.OfType<AllowAnonymousAttribute>().Any() is null or true;
}