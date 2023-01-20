using FluentValidation;

namespace ECommerce1.Models.Validators
{
    public static class IRuleBuilderExtension
    {
        public static IRuleBuilderOptions<T1, string> ValidCheckString<T1>(
            this IRuleBuilder<T1, string> builder,
            string propertyName = "Property",
            int min = 0,
            int max = 64)
        {
            if (min == 0)
            {
                return builder.MaximumLength(max).WithMessage($"Maximum {propertyName} length is {max} characters!");
            }

            return builder
                .ValidCheck(propertyName)
                .MinimumLength(min).WithMessage($"Minimum {propertyName} length is {min} characters!")
                .MaximumLength(max).WithMessage($"Maximum {propertyName} length is {max} characters!");
        }

        public static IRuleBuilderOptions<T1, T2> ValidCheck<T1, T2>(
            this IRuleBuilder<T1, T2> builder,
            string propertyName = "Property")
        {
            return builder
                .NotNull().WithMessage($"{propertyName} is empty!")
                .NotEmpty().WithMessage($"{propertyName} is empty!");
        }
    }
}
