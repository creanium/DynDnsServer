using System.Text.Json;
using Ardalis.Result;
using DigitalOcean.API;
using DigitalOcean.API.Models.Requests;
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

		var existingRecords = await _client.DomainRecords.GetAll(targetDomain.Name);
		logger.LogDebug("Got {RecordCount} domain record(s) for {DomainName}: {Records}", existingRecords?.Count, targetDomain.Name, JsonSerializer.Serialize(existingRecords));

		if (existingRecords is null)
		{
			logger.LogWarning("{Domain} records not be found in configured DigitalOcean account. Did find: {FoundDomainRecords}", domain, string.Join(", ", existingRecords?.Select(d => d.Name) ?? ["<none>"]));
			return Result.NotFound();
		}

		foreach (var record in _options.Records)
		{
			var targetRecord = existingRecords.FirstOrDefault(r => r.Name.Equals(record.Name, StringComparison.InvariantCultureIgnoreCase) 
			                                               && r.Name.Equals(record.RecordType, StringComparison.InvariantCultureIgnoreCase));

			if (targetRecord is null)
			{
				logger.LogInformation("Creating new record {RecordName} of type {RecordType} for {Domain}", record.Name, record.RecordType, targetDomain.Name);
				await _client.DomainRecords.Create(targetDomain.Name, new DomainRecord
				{
					Name = record.Name,
					Type = record.RecordType,
					Data = ip
				});
				continue;
			}

			logger.LogInformation("Updating record {RecordName} of type {RecordType} for {Domain}", targetRecord.Name, targetRecord.Type, targetDomain.Name);
			var updateRecord = new UpdateDomainRecord
			{
				Data = ip
			};
			
			await _client.DomainRecords.Update(targetDomain.Name, targetRecord.Id, updateRecord);
		}
		
		return Result.Success();
	}
}