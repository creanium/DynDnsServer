using Ardalis.Result;
using DynDns.Enum;
using DynDns.Models.Options;
using Microsoft.Extensions.Options;

namespace DynDns.Services;

public class NameserverUpdateService : INameserverUpdater
{
	private readonly ILogger<NameserverUpdateService> _logger;
	private readonly IOptions<ApplicationOptions> _options;
	private readonly ILoggerFactory _loggerFactory;

	public NameserverUpdateService(ILogger<NameserverUpdateService> logger, IOptionsSnapshot<ApplicationOptions> options, ILoggerFactory loggerFactory)
	{
		_logger = logger;
		_options = options;
		_loggerFactory = loggerFactory;
		
		_logger.LogDebug("Instantiated {ServiceName}", GetType().Name);
		_logger.LogDebug("Options: {@Options}", _options.Value);
	}


	public async Task<Result> UpdateDomain(string domainName, string ip)
	{
		_logger.LogDebug("Updating {Domain} with {Ip}", domainName, ip);
		var domainOptions = _options.Value.Domains.FirstOrDefault(d => d.Name.Equals(domainName, StringComparison.InvariantCultureIgnoreCase));
		
		if (domainOptions is null)
		{
			_logger.LogWarning("No domain options found for {Domain}", domainName);
			return Result.Invalid(new ValidationError("Domain", $"No domain options found for {domainName}"));
		}
		
		var updater = GetUpdater(domainOptions);
		_logger.LogDebug("Instantiated {UpdaterName}", updater.GetType().Name);
		_logger.LogInformation("Updating {Domain} with {Ip} using {ProviderName}", domainName, ip, domainOptions.Provider);
		
		return await updater.UpdateDomain(domainName, ip);
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