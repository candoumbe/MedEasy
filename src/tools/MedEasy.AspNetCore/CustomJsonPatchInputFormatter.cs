namespace MedEasy.AspNetCore;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;


/// <summary>
/// Static class used to build a JsonPatchInputFormatter
/// </summary>
public static class CustomJsonPatchInputFormatter
{
    /// <summary>
    /// Builds a <see cref="NewtonsoftJsonInputFormatter"/> isntance.
    /// </summary>
    /// <returns></returns>
    public static NewtonsoftJsonPatchInputFormatter GetJsonPatchInputFormatter()
    {
        var builder = new ServiceCollection()
            .AddLogging()
            .AddMvc()
            .AddNewtonsoftJson()
            .Services.BuildServiceProvider();

        return builder
            .GetRequiredService<IOptions<MvcOptions>>()
            .Value
            .InputFormatters
            .OfType<NewtonsoftJsonPatchInputFormatter>()
            .First();
    }
}