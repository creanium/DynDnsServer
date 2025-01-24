using System.Net;
using Ardalis.Result;
using DynDns.Enum;
using DynDns.Models.Request;
using DynDns.Services;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace DynDns.Endpoints;

[Authorize]
[HttpGet("/nic/update")]
public class UpdateEndpoint(ILogger<UpdateEndpoint> logger, NameserverUpdateService updateService) : Endpoint<UpdateRequest, ReturnCode>
{
	public override async Task HandleAsync(UpdateRequest req, CancellationToken ct)
	{
		logger.LogInformation("Received request to update {Hostname} with IP {IpAddress}", req.Hostname, req.IpAddress);

		try
		{
			var updateResult = await updateService.UpdateDomain(req.Hostname, req.IpAddress);

			if (updateResult.Status is ResultStatus.Invalid or ResultStatus.NotFound)
			{
				await SendStringAsync(ReturnCode.HostNotFound, cancellation: ct);
				return;
			}
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Error updating {Hostname} with IP {IpAddress}", req.Hostname, req.IpAddress);
			await SendStringAsync(ReturnCode.SystemError, cancellation: ct);
			return;
		}
		
		// https://help.dyn.com/remote-access-api/return-codes/
		await SendStringAsync(ReturnCode.Good, cancellation: ct);
	}
}