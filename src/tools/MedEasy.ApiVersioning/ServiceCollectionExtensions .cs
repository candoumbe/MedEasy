namespace MedEasy.ApiVersioning;

using Microsoft.Extensions.DependencyInjection;

public class ServiceeCollectionExtensions
{
    public static IServiceCollection AddCustomApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.UseApiBehavior = true;
            options.ReportApiVersions = true;
            options.ApiVersionSelector = new CurrentImplementationApiVersionSelector(options);
            options.DefaultApiVersion = new ApiVersion(2, 0);
            options.ApiVersionReader = new HeaderApiVersionReader();
        });
        services.AddVersionedApiExplorer(
            options =>
            {
                    // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
                    // note: the specified format code will format the version as "'v'major[.minor][-status]"
                    options.GroupNameFormat = "'v'VVV";

                    // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                    // can also be used to control the format of the API version in route templates
                    options.SubstituteApiVersionInUrl = true;
            });


        return services;
    }
}
