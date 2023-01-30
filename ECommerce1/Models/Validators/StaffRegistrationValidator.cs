using ECommerce1.Models.ViewModels;
using FluentValidation;

namespace ECommerce1.Models.Validators
{
    public class StaffRegistrationValidator : AbstractValidator<StaffCredentials>
    {
        public StaffRegistrationValidator()
        {
            RuleFor(c => c.Email)
                .ValidCheckString("Email", 3, 320)
                .EmailAddress().WithMessage("Not valid e-mail address!");

            RuleFor(c => c.Password)
                .ValidCheckString("Password", 8, 256)
                .Matches(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d#?!@$%^&*-_]{8,}$").WithMessage("Password must contain at least 1 letter and 1 digit!");

            RuleFor(c => c.PasswordConfirmation)
                .Equal(c => c.Password).WithMessage("Passwords do not match!");

            RuleFor(c => c.PhoneNumber)
                .ValidCheckString("Phone number", 3, 15)
                .Matches(@"^[\+]?[(]?[0-9]{3}[)]?[-\s\.]?[0-9]{3}[-\s\.]?[0-9]{4,6}$").WithMessage("Not valid phone number");

            RuleFor(c => c.DisplayName)
                .ValidCheckString("Display name", 1, 32);
        }
    }
}
