using DynDns.Authentication;
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

builder.Services.AddOptionsWithValidateOnStart<DynDnsServerOptions>()
	.BindConfiguration(DynDnsServerOptions.SectionName);

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

app.MapGet("/", () => "Hello World!");

app.Run();
