namespace Identity.CQRS.Handlers.Queries;

using AutoMapper.QueryableExtensions;

using Identity.CQRS.Queries.Accounts;
using Identity.DTO;
using Identity.Objects;

using MedEasy.DAL.Interfaces;

using MediatR;

using Optional;

using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Handles <see cref="GetOneAccountInfoByUsernameQuery"/> query
/// </summary>
public class HandleGetOneAccountInfoByUsernameQuery : IRequestHandler<GetOneAccountInfoByUsernameQuery, Option<AccountInfo>>
{
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IExpressionBuilder _expressionBuilder;

    /// <summary>
    /// Builds a new 
    /// </summary>
    /// <param name="unitOfWorkFactory"></param>
    /// <param name="expressionBuilder"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public HandleGetOneAccountInfoByUsernameQuery(IUnitOfWorkFactory unitOfWorkFactory, IExpressionBuilder expressionBuilder)
    {
        _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
        _expressionBuilder = expressionBuilder ?? throw new ArgumentNullException(nameof(expressionBuilder));
    }

    ///<inheritdoc/>
    public async Task<Option<AccountInfo>> Handle(GetOneAccountInfoByUsernameQuery request, CancellationToken cancellationToken)
    {
        Expression<Func<Account, AccountInfo>> selector = _expressionBuilder.GetMapExpression<Account, AccountInfo>();

        using IUnitOfWork uow = _unitOfWorkFactory.NewUnitOfWork();

        return await uow.Repository<Account>().SingleOrDefaultAsync(selector,
                                                                    (Account x) => x.Username == request.Data,
                                                                    cancellationToken)
                                              .ConfigureAwait(false);
    }
}
