using System.ComponentModel.DataAnnotations;

namespace Tareas.Models
{
    public class RegistroViewModel
    {
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [EmailAddress(ErrorMessage = "Email valido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
