using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace DynDns.Models.Options;

[UsedImplicitly]
public class AuthenticationOptions
{
	public const string SectionName = "Authentication";
	
	[Required]
	public required List<UserCredential> Logins { get; set; }
}