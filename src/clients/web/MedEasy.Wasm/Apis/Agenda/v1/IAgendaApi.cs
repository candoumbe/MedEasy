namespace MedEasy.Wasm.Apis.Agenda.v1;
using MedEasy.RestObjects;

using Refit;


/// <summary>
/// Describe the refit
/// </summary>
[Headers("api-version:1.0")]
public interface IAgendaApi
{
    /// <summary>
    /// Gets a page of appointments
    /// </summary>
    /// <param name="page"></param>
    /// <param name="pageSize"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Get("/appointments")]
    Task<IApiResponse<Page<Browsable<AppointmentModel>>>> ReadPage(int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Schedules a new appointment.
    /// </summary>
    /// <param name="accessToken">access token</param>
    /// <param name="appointment">Data of the new appointment</param>
    /// <returns>The created <see cref="AppointmentModel"/> wrapped inside a <see cref="IApiResponse{T}"/></returns>
    [Post("/appointments")]
    Task<IApiResponse<Browsable<AppointmentModel>>> Schedule([Body] NewAppointmentModel appointment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an appointment by its id
    /// </summary>
    /// <param name="accessToken"></param>
    /// <param name="id">identifier of the appointment to delete</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Delete("/appointments/{id}")]
    Task<IApiResponse> Delete(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an <see cref="AppointmentModel"/> by its <paramref name="id"/>
    /// </summary>
    /// <param name="accessToken"></param>
    /// <param name="id">identifier of the appointment to get</param>
    /// <param name="cancellationToken"></param>
    [Get("/appointments/{id}")]
    Task<IApiResponse<Browsable<AppointmentModel>>> GetById(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an <see cref="AppointmentModel"/> by its <paramref name="id"/>
    /// </summary>
    /// <param name="accessToken"></param>
    /// <param name="id">identifier of the appointment to get</param>
    /// <param name="cancellationToken"></param>
    [Get("/appointments/search")]
    Task<IApiResponse<Page<Browsable<AppointmentModel>>>> Search([Query] int page,
                                                      [Query] int pageSize,
                                                      [Query] SearchAppointmentModel model,
                                                      CancellationToken cancellationToken = default);


}
