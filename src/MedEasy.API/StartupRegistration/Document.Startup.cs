using Microsoft.Extensions.DependencyInjection;
using MedEasy.Handlers.Doctor.Queries;
using MedEasy.Handlers.Doctor.Commands;
using MedEasy.API.Controllers;
using MedEasy.Handlers.Core.Document.Queries;
using MedEasy.Handlers.Core.Queries;
using MedEasy.Queries.Document;
using MedEasy.DTO;
using MedEasy.Objects;
using System;
using MedEasy.Handlers.Document.Queries;

namespace MedEasy.API.StartupRegistration
{
    /// <summary>
    /// Wraps the dependency injection registration for <see cref="DocumentsController"/> dependencies.
    /// </summary>
    public static class DocumentStartupRegistration
    {
        /// <summary>
        /// Registers all dependencies needed by <see cref="DocumentsController"/>
        /// </summary>
        public static void AddDocumentsControllerDependencies(this IServiceCollection services)
        {

            services.AddScoped<IHandleGetOneDocumentMetadataInfoByIdQuery, HandleGetOneDocumentMetadataInfoByIdQuery>();
            services.AddScoped<IHandleGetManyDocumentsQuery, HandleGetManyDocumentMetadataInfosQuery>();


        }

    }
}
