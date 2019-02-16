using FluentValidation;
using Identity.DTO;
using Identity.Objects;
using MedEasy.DAL.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Validators
{
    public class NewAccountInfoValidator : AbstractValidator<NewAccountInfo>
    {
        public NewAccountInfoValidator(IUnitOfWorkFactory uowFactory, ILogger<NewAccountInfoValidator> logger)
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .MustAsync(async (email, ct) =>
                {
                    using (IUnitOfWork uow = uowFactory.NewUnitOfWork())
                    {
                        return !await uow.Repository<Account>().AnyAsync(x => x.Email == email, ct)
                            .ConfigureAwait(false);
                    }
                });

            RuleFor(x => x.Password)
                .NotNull();

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty()
                .Equal(x => x.Password);

            RuleFor(x => x.Username)
                .NotEmpty()
                .MustAsync(async (username, ct) =>
                {
                    logger?.LogDebug($"Validating username <{username}>");
                    using (IUnitOfWork uow = uowFactory.NewUnitOfWork())
                    {
                        bool alreadyUsed = await uow.Repository<Account>().AnyAsync(x => x.UserName == username, ct)
                            .ConfigureAwait(false);

                        logger?.LogDebug($"Username <{username}> {(alreadyUsed ? string.Empty:"not ")}already used");

                        return !alreadyUsed;
                    }
                });

        }
    }
}
