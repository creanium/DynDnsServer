using FastEndpoints;
using JetBrains.Annotations;

namespace DynDns.Models.Request;

[UsedImplicitly]
public class UpdateRequest
{
	[BindFrom("hostname")]
	public string? Hostname { get; set; }
	
	[BindFrom("myip")]
	public string? IpAddress { get; set; }
}