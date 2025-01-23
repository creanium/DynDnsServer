using System.Net;
using DynDns.Enum;
using DynDns.Models.Request;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace DynDns.Endpoints;

[Authorize]
[HttpGet("/nic/update")]
public class UpdateEndpoint(ILogger<UpdateEndpoint> logger) : Endpoint<UpdateRequest, ReturnCode>
{
	public override async Task HandleAsync(UpdateRequest req, CancellationToken ct)
	{
		logger.LogInformation("Received request to update {Hostname} with IP {IpAddress}", req.Hostname, req.IpAddress);

		// https://help.dyn.com/remote-access-api/return-codes/
		await SendStringAsync(ReturnCode.Good, cancellation: ct);
	}
}