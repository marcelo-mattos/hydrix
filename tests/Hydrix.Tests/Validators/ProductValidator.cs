using FluentValidation;
using Hydrix.Tests.Database.Entity;
using Microsoft.Extensions.Localization;
using System;
using System.Globalization;

namespace Hydrix.Tests.Validators
{
    /// <summary>
    /// Provides validation rules for Product objects to ensure they meet defined business and data integrity
    /// requirements.
    /// </summary>
    /// <remarks>This class extends the AbstractValidator class, allowing integration with validation
    /// frameworks to enforce constraints specific to Product instances. Use this validator to centralize and
    /// standardize validation logic for products throughout the application.</remarks>
    public class ProductValidator : AbstractValidator<Product>
    {
        /// <summary>
        /// Initializes a new instance of the ProductValidator class.
        /// </summary>
        /// <param name="localizer">The localizer used for retrieving localized error messages.</param>
        public ProductValidator(IStringLocalizer localizer)
        {
            When(x => x.Type == "O", () =>
            {
                RuleFor(x => x.Token)
                    .NotEmpty()
                    .Matches(@"^\d{6}$")
                    .WithMessage(_ => localizer["TokenMustBeSixDigitsForTypeO"]);
            });

            When(x => x.Type == "X", () =>
            {
                RuleFor(x => x.Token)
                    .Must(token => string.IsNullOrWhiteSpace(token))
                    .WithMessage(localizer["TokenMustBeEmptyForTypeX"]);
            });

            When(x => x.Type == "K", () =>
            {
                RuleFor(x => x.Token)
                    .NotEmpty()
                    .Must(token =>
                        DateTime.TryParseExact(
                            token,
                            "yyyy-MM-dd",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out _))
                    .WithMessage(localizer["TokenMustBeValidDateForTypeK"]);
            });
        }
    }
}