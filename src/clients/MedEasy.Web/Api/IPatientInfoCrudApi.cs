using MedEasy.DTO;
using MedEasy.DTO.Search;
using MedEasy.RestObjects;
using Microsoft.AspNetCore.JsonPatch;
using Refit;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MedEasy.Web.Api
{
    /// <summary>
    /// Endpoint that handle <see cref="PatientInfo"/> resources.
    /// </summary>
    public interface IPatientInfoCrudApi
    {
        /// <summary>
        /// Creates a new <see cref="PatientInfo"/> resource
        /// </summary>
        /// <param name="info">data of the resource to create</param>
        /// <returns>The created resource</returns>
        [Post("/patients")]
        Task<BrowsableResource<PatientInfo>> Create([Body]CreatePatientInfo info, CancellationToken cancellationToken);

        /// <summary>
        /// Gets a 
        /// </summary>
        /// <param name="paginationConfiguration"></param>
        /// <returns></returns>
        [Get("/patients?page={page}&pageSize={pageSize}")]
        Task<GenericPagedGetResponse<BrowsableResource<PatientInfo>>> GetMany(PaginationConfiguration paginationConfiguration, CancellationToken cancellation);

        /// <summary>
        /// Gets one resource by its <see cref="id"/>
        /// </summary>
        /// <param name="id">id of the resource to get</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Get("/patients/{id}")]
        Task<BrowsableResource<PatientInfo>> GetOne(Guid id, CancellationToken cancellationToken);

        
        [Patch("/patients/{id}")]
        Task Patch(Guid id, [Body] JsonPatchDocument<PatientInfo> changes, CancellationToken cancellation) ;

        [Delete("{id}")]
        Task Delete(Guid id, CancellationToken cancellationToken);

        [Get("/patients/search")]
        Task<GenericPagedGetResponse<BrowsableResource<PatientInfo>>> Search(SearchPatientInfo search, CancellationToken cancellationToken);
    }
}
