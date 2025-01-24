using Ardalis.SmartEnum;

namespace DynDns.Enum;

public sealed class ReturnCode(string name, string value) : SmartEnum<ReturnCode, string>(name, value)
{
	public static readonly ReturnCode BadAuth = new ReturnCode("badauth", "badauth");

	public static readonly ReturnCode Good = new ReturnCode("good", "good");
	public static readonly ReturnCode NoChange = new ReturnCode("nochg", "nochg");

	public static readonly ReturnCode InvalidDomain = new ReturnCode("notfqdn", "notfqdn");
	public static readonly ReturnCode HostNotFound = new ReturnCode("nohost", "nohost");
	public static readonly ReturnCode TooManyHosts = new ReturnCode("numhost", "numhost");
	public static readonly ReturnCode BlockedForAbuse = new ReturnCode("abuse", "abuse");

	public static readonly ReturnCode BadAgent = new ReturnCode("badagent", "badagent");

	public static readonly ReturnCode DnsUpdateError = new ReturnCode("dnserr", "dnserr");
	public static readonly ReturnCode SystemError = new ReturnCode("911", "911");
}