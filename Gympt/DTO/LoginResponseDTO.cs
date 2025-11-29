using Gympt.DTO;

namespace Gympt.DTO
{
    public class LoginResponseDTO
    {
        public string token { get; set; } = string.Empty; 
        public UserLoggedDTO user { get; set; } = new UserLoggedDTO();
    }
}

