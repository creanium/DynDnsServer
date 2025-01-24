using Ardalis.SmartEnum;

namespace DynDns.Enum;

public sealed class NameserverProvider(string name, string value) : SmartEnum<NameserverProvider, string>(name, value)
{
	public static readonly NameserverProvider DigitalOcean = new("DigitalOcean", "DigitalOcean");
}
