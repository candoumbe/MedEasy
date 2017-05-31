using Microsoft.AspNetCore.Mvc;

/// <summary>
/// An <see cref="ActionResult"/> which returns an Accepted (202) response with a Location header.
/// </summary>
public class AcceptedAtRouteResult : CreatedAtRouteResult
{
	/// <summary>
    /// Builds a new <see cref="AcceptedAtRouteResult"/> instance. 
    /// </summary>
    /// <param name="routeName"></param>
    /// <param name="routeValues"></param>
    /// <param name="value"></param>
    public AcceptedAtRouteResult(string routeName, object routeValues, object value) : base(routeName, routeValues, value)
    {
        StatusCode = 202;
    }

}