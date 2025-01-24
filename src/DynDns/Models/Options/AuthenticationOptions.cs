namespace DynDns.Models.Options;

public class AuthenticationOptions
{
	public const string SectionName = "Authentication";
	
	public List<UserCredential> Logins { get; set; }
}