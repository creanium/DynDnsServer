using System.ComponentModel.DataAnnotations;

namespace DynDns.Models.Options;

public sealed class UserCredential
{
	[Required]
	public string Username { get; set; }
	
	[Required]
	public string Password { get; set; }
}