using FluentValidation;
using TestCrud.Entities;
using TestCrud.Models;

public class UserLoginValidator : AbstractValidator<UserLogin>
{
    public UserLoginValidator()
    {
        RuleFor(user => user.Email)
            .NotEmpty().WithMessage("Email is a required field")
            .EmailAddress().WithMessage("Please provide a valid email address");

        RuleFor(user => user.Password)
            .NotEmpty().WithMessage("Password is a required field")
            .MinimumLength(6).WithMessage("Password should have a minimum length of 6 characters");

    }
}