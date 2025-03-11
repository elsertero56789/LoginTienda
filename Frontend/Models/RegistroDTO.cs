using System.ComponentModel.DataAnnotations;

namespace Frontend.Models
{
    public class RegistroDTO
    {
        [Required(ErrorMessage = "El atributo Email es Requerido")]
        public string? Email { get; set; }
        [Required(ErrorMessage = "El atributo Password es requerido")]
        public string? Password { get; set; }
    }
}
