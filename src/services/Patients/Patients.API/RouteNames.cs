namespace Patients.API.Routing
{
    /// <summary>
    /// Wrapper for strongly typed routenames.
    /// </summary>
    public static class RouteNames
    {
        /// <summary>
        /// Name of the route that retuns the root URL.
        /// </summary>
        public const string Default = nameof(Default);

        /// <summary>
        /// Name of the route that returns all resources (i.e. "api/{resource}/")
        /// </summary>
        public const string DefaultGetAllApi = nameof(DefaultGetAllApi);

        /// <summary>
        /// Name of the route to get a resource by it's id (i.e. "api/{resource}/{id}")
        /// </summary>
        public const string DefaultGetOneByIdApi = nameof(DefaultGetOneByIdApi);


        /// <summary>
        /// Name of the route to get subresources by it's parent resource's id (i.e. "api/{controller}/{id}/{action}")
        /// </summary>
        public const string DefaultGetAllSubResourcesByResourceIdApi = nameof(DefaultGetAllSubResourcesByResourceIdApi);

        /// <summary>
        /// Name of the route to search resources at a specified endpoint ("api/{resource}/search").
        /// </summary>
        public const string DefaultSearchResourcesApi = nameof(DefaultSearchResourcesApi);

        /// <summary>
        /// Name of the route to get one subresource by it's parent resource's id (ex "api/{controller}/{id}/{action}/{subResourceId}")
        /// </summary>
        public const string DefaultGetOneSubResourcesByResourceIdAndSubresourceIdApi = nameof(DefaultGetOneSubResourcesByResourceIdAndSubresourceIdApi);
    }
}
