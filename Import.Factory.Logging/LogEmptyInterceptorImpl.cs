using Log.Interceptors;
using Microsoft.Extensions.Logging;

namespace Import.Factory.Logging;

internal class LogEmptyInterceptorImpl<T>(ILogger logger) : LogInterceptor(logger), ILogger<T>
{
}
