using Microsoft.AspNetCore.Components;
using MedEasy.Web.Core;

/// <summary>
/// Base class for component that requires a connected user.
/// </summary>
/// <remarks>
/// 
/// </remarks>
public class AuthenticatedComponentBase : ComponentBase
{

    [Inject] public NavigationManager NavigationManager { get; set; }
}