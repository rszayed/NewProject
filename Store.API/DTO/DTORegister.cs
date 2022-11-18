using System.ComponentModel.DataAnnotations;

namespace Store.API.DTO
{
    public class DTORegister
    {
        [Required]
        public string? Username { get; set; }
        [StringLength(8,MinimumLength =4,ErrorMessage ="дольжен порол от 4 до 8 букв или цифр")]
        public string? Password { get; set; }
        [Required]
        public string? ConfirmPassword { get; set; }

    }
}
