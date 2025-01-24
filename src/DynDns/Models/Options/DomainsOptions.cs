namespace DynDns.Models.Options;

public sealed class DomainsOptions
{
	public const string SectionName = "Domains";
	
	public List<DomainOptions> Domains { get; set; }
}