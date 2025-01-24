using System.ComponentModel.DataAnnotations;
using DynDns.Enum;

namespace DynDns.Models.Options;

public class DomainOptions
{
	[Required]
	public string Name { get; set; }
	
	[Required]
	public string Provider { get; set; }

	public string ApiKey { get; set; }
	
	public List<DomainRecordOption> Records { get; set; }
}