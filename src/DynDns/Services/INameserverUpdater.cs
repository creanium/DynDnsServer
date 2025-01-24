using Ardalis.Result;

namespace DynDns.Services;

public interface INameserverUpdater
{
	Task<Result> UpdateDomain(string domainName, string ip);
}