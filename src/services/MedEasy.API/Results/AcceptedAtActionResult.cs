using static Microsoft.AspNetCore.Http.StatusCodes;
using Microsoft.AspNetCore.Mvc;


/// <summary>
/// An <see cref="ActionResult"/> which is an Accepted (202) response with a Location header.
/// </summary>
public class AcceptedAtActionResult : CreatedAtActionResult
{
    /// <summary>
    /// Builds a new <see cref="AcceptedAtActionResult"/> instance.
    /// </summary>
    /// <param name="actionName">Name of the action to use for generating the URL</param>
    /// <param name="controllerName">Name of the controller to use for generating the URL</param>
    /// <param name="routeValues"></param>
    /// <param name="value">Content of the result</param>
    public AcceptedAtActionResult(string actionName, string controllerName, object routeValues, object value) : base(actionName, controllerName, routeValues, value)
    {
        StatusCode = Status202Accepted;
    }
}