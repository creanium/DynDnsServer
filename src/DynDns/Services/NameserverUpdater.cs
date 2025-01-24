using Ardalis.Result;
using DynDns.Models.Options;

namespace DynDns.Services;

public abstract class NameserverUpdater : INameserverUpdater
{
	protected readonly DomainOptions _options;

	protected NameserverUpdater(DomainOptions options)
	{
		_options = options;
	}
	
	public abstract Task<Result> UpdateDomain(string domainName, string ip);
}