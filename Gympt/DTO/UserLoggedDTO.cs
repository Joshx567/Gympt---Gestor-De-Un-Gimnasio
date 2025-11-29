namespace Gympt.DTO
{
    public class UserLoggedDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool MustChangePassword { get; set; }
    }

}
