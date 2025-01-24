using System.ComponentModel.DataAnnotations;

namespace DynDns.Models.Options;

public class DomainOptions
{
	[Required]
	public string Name { get; set; }
	
	[Required]
	public string Provider { get; set; }

	public string ApiKey { get; set; }
	
	
}