using System.Text.Json;
using Ardalis.Result;
using DigitalOcean.API;
using DynDns.Models.Options;
using Microsoft.Extensions.Options;

namespace DynDns.Services;

public class DigitalOceanUpdater(DomainOptions options, ILogger<DigitalOceanUpdater> logger) : NameserverUpdater(options)
{
	private readonly DigitalOceanClient _client = new(options.ApiKey);
	
	public override async Task<Result> UpdateAsync(string domain, string ip)
	{
		logger.LogDebug("Updating {Domain} with {Ip} in DigitalOcean", domain, ip);
		var domains = await _client.Domains.GetAll();
		
		var targetDomain = domains?.FirstOrDefault(d => d.Name == domain);

		if (targetDomain is null)
		{
			logger.LogWarning("{Domain} could not be found in configured DigitalOcean account. Did find: {FoundDomains}", domain, string.Join(", ", domains?.Select(d => d.Name) ?? ["<none>"]));
			return Result.NotFound();
		}

		logger.LogDebug("Found {Domain} in DigitalOcean", targetDomain.Name);

		var records = await _client.DomainRecords.GetAll(targetDomain.Name);
		logger.LogDebug("Got {RecordCount} domain record(s) for {DomainName}: {Records}", records?.Count, targetDomain.Name, JsonSerializer.Serialize(records));

		if (records is null)
		{
			logger.LogWarning("{Domain} records not be found in configured DigitalOcean account. Did find: {FoundDomainRecords}", domain, string.Join(", ", records?.Select(d => d.Name) ?? ["<none>"]));
			return Result.NotFound();
		}
		
		return Result.Success();
	}
}