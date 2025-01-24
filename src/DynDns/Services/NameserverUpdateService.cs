using Ardalis.Result;
using DynDns.Enum;
using DynDns.Models.Options;
using Microsoft.Extensions.Options;

namespace DynDns.Services;

public class NameserverUpdateService : INameserverUpdater
{
	private readonly ILogger<NameserverUpdateService> _logger;
	private readonly IOptions<DynDnsServerOptions> _options;
	private readonly ILoggerFactory _loggerFactory;

	public NameserverUpdateService(ILogger<NameserverUpdateService> logger, IOptionsSnapshot<DynDnsServerOptions> options, ILoggerFactory loggerFactory)
	{
		_logger = logger;
		_options = options;
		_loggerFactory = loggerFactory;
		
		_logger.LogDebug("Instantiated {ServiceName}", GetType().Name);
		_logger.LogDebug("Options: {@Options}", _options.Value);
	}


	public async Task<Result> UpdateAsync(string domain, string ip)
	{
		_logger.LogDebug("Updating {Domain} with {Ip}", domain, ip);
		var domainOptions = _options.Value.Domains.FirstOrDefault(d => d.Name.Equals(domain, StringComparison.InvariantCultureIgnoreCase));
		
		if (domainOptions is null)
		{
			_logger.LogWarning("No domain options found for {Domain}", domain);
			return Result.Invalid(new ValidationError("Domain", $"No domain options found for {domain}"));
		}
		
		var updater = GetUpdater(domainOptions);
		_logger.LogDebug("Instantiated {UpdaterName}", updater.GetType().Name);
		_logger.LogInformation("Updating {Domain} with {Ip} using {ProviderName}", domain, ip, domainOptions.Provider);
		
		return await updater.UpdateAsync(domain, ip);
	}
	
	private INameserverUpdater GetUpdater(DomainOptions domainOptions)
	{
		_logger.LogDebug("Getting {ProviderName} provider for {Domain}", domainOptions.Provider, domainOptions.Name);

		var provider = NameserverProvider.FromName(domainOptions.Provider);
		
		return provider.Name switch
		{
			nameof(NameserverProvider.DigitalOcean) => new DigitalOceanUpdater(domainOptions, _loggerFactory.CreateLogger<DigitalOceanUpdater>()),
			_ => throw new NotImplementedException()
		};
	}
}