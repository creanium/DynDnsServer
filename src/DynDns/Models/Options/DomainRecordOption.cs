using System.ComponentModel.DataAnnotations;

namespace DynDns.Models.Options;

public sealed class DomainRecordOption
{
	[Required]
	public string Name { get; set; }
	
	[Required]
	public string RecordType { get; set; }
	
	[Required]
	public string Ttl { get; set; }
}