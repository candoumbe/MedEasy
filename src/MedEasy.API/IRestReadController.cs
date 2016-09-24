using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MedEasy.API.Controllers
{
    /// <summary>
    /// Interface for rest controllers that are read only endpoints
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IRestReadController<TKey>
    {

        Task<IActionResult> Get(TKey id);
    }
}