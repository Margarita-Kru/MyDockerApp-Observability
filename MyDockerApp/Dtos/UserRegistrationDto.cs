using System.ComponentModel.DataAnnotations;

namespace MyDockerApp.Dtos
{
    public class UserRegistrationDto
    {
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Длина от 3 до 20 символов")]
        public required string UserName { get; set; }

        [EmailAddress]
        public required string Email { get; set; }

        [Range(18, 99, ErrorMessage = "Возраст от 18 до 99 лет")]
        public int Age { get; set; }
        
        [Required(ErrorMessage = "Укажите дату рождения")]        
        public DateOnly? DateBirth { get; set; }

        public bool IsSubscribedToNews { get; set; }

        public required IFormFile Avatar { get; set; }

    }
}
