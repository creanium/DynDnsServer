using DynDns.Authentication;
using DynDns.Models.Options;
using Serilog;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication;
using AuthenticationOptions = DynDns.Models.Options.AuthenticationOptions;

var builder = WebApplication.CreateBuilder(args);

//Add support to logging with Serilog
builder.Host.UseSerilog((context, configuration) =>
	configuration.ReadFrom.Configuration(context.Configuration));

builder.Services
	.Configure<AuthenticationOptions>(builder.Configuration.GetSection(AuthenticationOptions.SectionName))
	.Configure<DomainsOptions>(builder.Configuration.GetSection(DomainsOptions.SectionName));

builder.Services
	.AddFastEndpoints()
	.AddAuthorization()
	.AddAuthentication(BasicAuthProvider.SchemeName)
	.AddScheme<AuthenticationSchemeOptions, BasicAuthProvider>(BasicAuthProvider.SchemeName, null);

var app = builder.Build();

//Add support to logging request with SERILOG
app.UseSerilogRequestLogging();

app.UseAuthentication()
	.UseAuthorization()
	.UseFastEndpoints();

app.MapGet("/", () => "Hello World!");

app.Run();
