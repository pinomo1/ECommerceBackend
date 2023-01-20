using ECommerce1.Models.ViewModels;
using FluentValidation;

namespace ECommerce1.Models.Validators
{
    public class ProductValidator : AbstractValidator<AddProductViewModel>
    {
        public ProductValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is empty");
            RuleFor(x => x.Price)
                .NotEmpty().WithMessage("Price is empty");
            RuleFor(x => x.Photos)
                .NotEmpty().WithMessage("Add at least 1 photo")
                .Must(photos => photos.Length >= 1).WithMessage("Must have at least 1 photo");
            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("The category id is empty");
        }
    }
}
