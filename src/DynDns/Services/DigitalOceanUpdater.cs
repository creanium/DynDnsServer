using System.Text.Json;
using Ardalis.Result;
using DigitalOcean.API;
using DigitalOcean.API.Models.Requests;
using DynDns.Models.Options;
using Domain = DigitalOcean.API.Models.Responses.Domain;

namespace DynDns.Services;

public class DigitalOceanUpdater(DomainOptions options, ILogger<DigitalOceanUpdater> logger) : NameserverUpdater(options)
{
	private readonly DigitalOceanClient _client = new(options.ApiKey);

	public override async Task<Result> UpdateDomain(string domainName, string ip)
	{
		logger.LogDebug("Updating {Domain} with {Ip} in DigitalOcean", domainName, ip);
		var domains = await _client.Domains.GetAll();

		var targetDomain = domains?.FirstOrDefault(d => d.Name == domainName);

		if (targetDomain is null)
		{
			logger.LogWarning("{Domain} could not be found in configured DigitalOcean account. Did find: {FoundDomains}", domainName, string.Join(", ", domains?.Select(d => d.Name) ?? ["<none>"]));
			return Result.NotFound();
		}

		logger.LogDebug("Found {Domain} in DigitalOcean", targetDomain.Name);

		var existingRecords = await _client.DomainRecords.GetAll(targetDomain.Name);
		logger.LogDebug("Got {RecordCount} domain record(s) for {DomainName}: {Records}", existingRecords?.Count ?? 0, targetDomain.Name, JsonSerializer.Serialize(existingRecords));

		if (existingRecords is null)
		{
			logger.LogWarning("{Domain} records not be found in configured DigitalOcean account. Did find: {FoundDomainRecords}", domainName, string.Join(", ", existingRecords?.Select(d => d.Name) ?? ["<none>"]));
			return Result.NotFound();
		}

		foreach (var recordConfig in _options.Records)
		{
			var targetRecord = existingRecords.FirstOrDefault(r => r.Name.Equals(recordConfig.Name, StringComparison.InvariantCultureIgnoreCase)
			                                                       && r.Type.Equals(recordConfig.RecordType, StringComparison.InvariantCultureIgnoreCase));

			if (targetRecord is null)
			{
				await CreateRecord(targetDomain, recordConfig, ip);
				continue;
			}

			await UpdateRecord(targetDomain, targetRecord, recordConfig, ip);
		}

		return Result.Success();
	}

	private async Task UpdateRecord(Domain targetDomain, DigitalOcean.API.Models.Responses.DomainRecord targetRecord, DomainRecordOption recordConfig, string ip)
	{
		logger.LogInformation("Updating record {RecordName} of type {RecordType} for {Domain}", targetRecord.Name, targetRecord.Type, targetDomain.Name);
		var updateRecord = new UpdateDomainRecord
		{
			Data = ip,
			Ttl = int.Parse(recordConfig.Ttl)
		};

		await _client.DomainRecords.Update(targetDomain.Name, targetRecord.Id, updateRecord);
	}

	private async Task CreateRecord(Domain targetDomain, DomainRecordOption domainRecord, string ip)
	{
		if (!domainRecord.CreateIfNotExists)
		{
			logger.LogWarning("No record with name {RecordName} of type {RecordType} for {Domain}. Application is configured not to create the record, skipping", 
				domainRecord.Name, domainRecord.RecordType, targetDomain.Name);
			return;
		}

		logger.LogInformation("Creating new record {RecordName} of type {RecordType} for {Domain}", domainRecord.Name, domainRecord.RecordType, targetDomain.Name);
		await _client.DomainRecords.Create(targetDomain.Name, new DomainRecord
		{
			Name = domainRecord.Name,
			Type = domainRecord.RecordType,
			Data = ip,
			Ttl = int.Parse(domainRecord.Ttl)
		});
	}
}