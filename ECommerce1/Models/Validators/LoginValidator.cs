using ECommerce1.Models.ViewModels;
using FluentValidation;

namespace ECommerce1.Models.Validators
{
    public class LoginValidator : AbstractValidator<LoginCredentials>
    {
        public LoginValidator()
        {
            RuleFor(c => c.Email)
                .ValidCheckString(min: 3, max: 320);

            RuleFor(c => c.Password)
                .ValidCheckString("Password", 8, 256)
                .Matches(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{8,}$").WithMessage("Password must contain at least 1 letter and 1 digit!");
        }
    }
}
