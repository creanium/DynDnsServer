using Ardalis.SmartEnum;

namespace DynDns.Enum;

public class ReturnCode(string name, string value) : SmartEnum<ReturnCode, string>(name, value)
{
	public static ReturnCode BadAuth = new ReturnCode("badauth", "badauth");
	
	public static ReturnCode Good = new ReturnCode("good", "good");
	public static ReturnCode NoChange = new ReturnCode("nochg", "nochg");
	
	public static ReturnCode InvalidDomain = new ReturnCode("notfqdn", "notfqdn");
	public static ReturnCode HostNotFound = new ReturnCode("nohost", "nohost");
	public static ReturnCode TooManyHosts = new ReturnCode("numhost", "numhost");
	public static ReturnCode BlockedForAbuse = new ReturnCode("abuse", "abuse");

	public static ReturnCode BadAgent = new ReturnCode("badagent", "badagent");
	
	public static ReturnCode DnsUpdateError = new ReturnCode("dnserr", "dnserr");
	public static ReturnCode SystemError = new ReturnCode("911", "911");
}