using System.ComponentModel.DataAnnotations;

namespace IdentityServerAspNetIdentity.Pages.Account.Create
{
    public class InputModel
    {
        [Required]
        public string? Username { get; set; }
        [Required]
        public string? Email { get; set; }
        [Required]
        public string? Password { get; set; }
        public string? ReturnUrl { get; set; }
        public string? Button { get; set; }
    }
}
