using ChatApp.Application.DTOs;
using FluentValidation;

namespace ChatApp.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Kullanıcı adı zorunlu.")
            .MinimumLength(3).WithMessage("En az 3 karakter.")
            .MaximumLength(30).WithMessage("En fazla 30 karakter.")
            .Matches("^[a-z0-9_]+$").WithMessage("Sadece küçük harf, rakam ve alt çizgi.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email zorunlu.")
            .EmailAddress().WithMessage("Geçerli bir email gir.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre zorunlu.")
            .MinimumLength(8).WithMessage("En az 8 karakter.")
            .Matches("[A-Z]").WithMessage("En az bir büyük harf.")
            .Matches("[0-9]").WithMessage("En az bir rakam.");

        RuleFor(x => x.DisplayName)
            .MaximumLength(50).WithMessage("En fazla 50 karakter.")
            .When(x => x.DisplayName is not null);
    }
}

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email zorunlu.")
            .EmailAddress().WithMessage("Geçerli bir email gir.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre zorunlu.");
    }
}

public class SendMessageRequestValidator : AbstractValidator<SendMessageRequest>
{
    public SendMessageRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Mesaj boş olamaz.")
            .MaximumLength(4000).WithMessage("En fazla 4000 karakter.");

        RuleFor(x => x.RoomId)
            .NotEmpty().WithMessage("Oda ID zorunlu.");
    }
}