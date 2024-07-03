using FluentValidation;
using TestCrud.Entities;

public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(user => user.Email)
            .NotEmpty().WithMessage("Email is a required field")
            .EmailAddress().WithMessage("Please provide a valid email address");

        RuleFor(user => user.Name)
            .NotEmpty().WithMessage("Name is a required field");

        RuleFor(user => user.Password)
            .NotEmpty().WithMessage("Password is a required field")
            .MinimumLength(6).WithMessage("Password should have a minimum length of 6 characters");

        RuleFor(user => user.Role)
            .NotEmpty().WithMessage("Role is a required field")
            .Must(role => role == "Admin" || role == "User")
            .WithMessage("Role must be either 'Admin' or 'User'");
    }
}
