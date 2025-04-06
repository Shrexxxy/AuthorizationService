using System.ComponentModel.DataAnnotations;

namespace IdentityService.Application.Model;

public class RegisterModel
{
    public string UserName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Password { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
}