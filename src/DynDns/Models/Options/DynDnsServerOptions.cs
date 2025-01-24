namespace DynDns.Models.Options;

public sealed class DynDnsServerOptions
{
	public const string SectionName = "DynDnsServer";
	
	public AuthenticationOptions Authentication { get; set; }
	public List<DomainOptions> Domains { get; set; }
}