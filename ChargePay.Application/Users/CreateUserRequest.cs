using System.ComponentModel.DataAnnotations;

namespace ChargePay.Application.Users;

public class CreateUserRequest
{
    [Required]
    [StringLength(80, MinimumLength = 2)]
    public string FirstName { get; set; } = null!;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string LastName { get; set; } = null!;

    [Required]
    public string Document { get; set; } = null!;

    [Required]
    [Phone]
    public string Phone { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    public DateTime BirthDate { get; set; }

    [Required]
    [RegularExpression("^\\d{6}$", ErrorMessage = "Senha deve conter exatamente 6 dígitos numéricos.")]
    public string Password { get; set; } = null!;
}
