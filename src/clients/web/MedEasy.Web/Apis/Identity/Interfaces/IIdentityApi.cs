namespace MedEasy.Web.Apis.Identity.Interfaces
{
    using global::Identity.Ids;
    using global::Identity.Models.v1;

    using MedEasy.RestObjects;

    using Refit;

    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Identity API interface
    /// </summary>
    public interface IIdentityApi
    {
        /// <summary>
        /// Creates a new <see cref="AccountModel"/>.
        /// </summary>
        /// <param name="newAccount">Data for the account to create</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [Post("/v1/accounts")]
        Task<ApiResponse<Browsable<AccountModel>>> CreateAccount([Body] NewAccountModel newAccount, CancellationToken ct = default);

        /// <summary>
        /// Get a page of <see cref="Browsable{AccountModel}"/>
        /// </summary>
        /// <param name="page">1-based index of the page to get</param>
        /// <param name="pageSize">number of elements per page.</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [Get("/v1/accounts?page={page}&pageSize={pageSize}")]
        Task<ApiResponse<GenericPagedGetResponse<Browsable<AccountModel>>>> GetAccountsPage([Query] uint page,
                                                                                            [Query] uint pageSize,
                                                                                            CancellationToken ct = default);


        /// <summary>
        /// Get an account with the specified id
        /// </summary>
        /// <param name="page">1-based index of the page to get</param>
        /// <param name="pageSize">number of elements per page.</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [Get("/v1/accounts/{id}")]
        Task<ApiResponse<Browsable<AccountModel>>> GetOneById(AccountId id,
                                                              CancellationToken ct = default);


        /// <summary>
        /// Authenticate the specified account
        /// </summary>
        /// <param name="login"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Post("/v1/auth/token")]
        Task<ApiResponse<BearerTokenModel>> Connect([Body] LoginModel login,
                                                    CancellationToken cancellationToken);
    }
}
