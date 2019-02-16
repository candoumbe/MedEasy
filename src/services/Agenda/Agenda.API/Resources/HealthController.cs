using Agenda.Objects;
using MedEasy.DAL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Agenda.API.Resources
{
    [Route("agenda/[controller]")]
    public class HealthController
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        /// <summary>
        /// Builds a new <see cref="HealthController" /> instance.
        /// </summary>
        /// <param name="unitOfWorkFactory"></param>
        public HealthController(IUnitOfWorkFactory unitOfWorkFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        /// <summary>
        /// Gets the current status of the API
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpHead("[action]")]
        [HttpGet("[action]")]
        public async Task<IActionResult> Status(CancellationToken ct = default)
        {
            IActionResult actionResult;
            try
            {
                using (IUnitOfWork uow = _unitOfWorkFactory.NewUnitOfWork())
                {
                    await uow.Repository<Participant>().AnyAsync(ct)
                        .ConfigureAwait(false);

                    actionResult = new NoContentResult();
                }
            }
            catch (Exception)
            {
                actionResult =  new StatusCodeResult(Status500InternalServerError);
            }

            return actionResult;
        }
    }
}
