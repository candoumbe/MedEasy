using Extensions.Hosting.AsyncInitialization;

using System.Threading.Tasks;

namespace MedEasy.AspnetCore.AsyncInitializers;

/// <summary>
/// Base class to extend in order to perform async operations during a service startup
/// </summary>
public abstract class AsyncBaseIntializer : IAsyncInitializer
{
    ///<inheritdoc/>
    public abstract Task InitializeAsync();
}
