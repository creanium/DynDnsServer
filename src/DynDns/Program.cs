using DynDns.Authentication;
using DynDns.Enum;
using DynDns.Models.Options;
using DynDns.Services;
using Serilog;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication;
using AuthenticationOptions = DynDns.Models.Options.AuthenticationOptions;

var builder = WebApplication.CreateBuilder(args);

//Add support to logging with Serilog
builder.Host.UseSerilog((context, configuration) =>
	configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddOptionsWithValidateOnStart<ApplicationOptions>()
	.BindConfiguration(ApplicationOptions.SectionName)
	.PostConfigure(opts =>
	{
		if (opts.Authentication.Logins.Count == 0)
		{
			throw new InvalidOperationException($"You must provide valid logins in the configuration file under the section [{ApplicationOptions.SectionName}:{AuthenticationOptions.SectionName}]");
		}
		
		var invalidProviders = opts.Domains.Where(d => !NameserverProvider.TryFromName(d.Provider, out _)).ToList();

		if (invalidProviders.Count == 0)
		{
			return;
		}

		var invalidProvidersList = string.Join(", ", invalidProviders.Select(d => d.Provider));
		var validProviders = string.Join(", ", NameserverProvider.List.Select(p => p.Name));
		throw new InvalidOperationException($"Invalid domain provider(s) found: {invalidProvidersList}. Valid providers are: {validProviders}");
	});

builder.Services
	.AddFastEndpoints()
	.AddAuthorization()
	.AddAuthentication(BasicAuthProvider.SchemeName)
	.AddScheme<AuthenticationSchemeOptions, BasicAuthProvider>(BasicAuthProvider.SchemeName, null);

builder.Services
	.AddScoped<NameserverUpdateService>();

var app = builder.Build();

//Add support to logging request with SERILOG
app.UseSerilogRequestLogging();

app.UseAuthentication()
	.UseAuthorization()
	.UseFastEndpoints();

app.MapGet("/", () => "Hello World!")
	.AllowAnonymous();

app.Run();