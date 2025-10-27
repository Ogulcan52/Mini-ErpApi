using ERP.Application.DTOs;
using FluentValidation;

namespace ERP.Application.Validation
{
    public class OrderCreateDtoValidator : AbstractValidator<OrderCreateDto>
    {
        public OrderCreateDtoValidator()
        {
            RuleFor(x => x.CustomerId).GreaterThan(0);
            RuleForEach(x => x.Items).SetValidator(new OrderItemCreateDtoValidator());
            RuleFor(x => x.Items).NotEmpty();
        }
    }

    public class OrderItemCreateDtoValidator : AbstractValidator<OrderItemCreateDto>
    {
        public OrderItemCreateDtoValidator()
        {
            RuleFor(x => x.ProductName).NotEmpty();
            RuleFor(x => x.Quantity).GreaterThan(0);
        }
    }
}
