namespace CEA_API.Models.DTOs
{
    public class UserDTO
    {
        public int IdUser { get; set; }

        public string NameUser { get; set; } = null!;

        public string EmailUser { get; set; } = null!;

        public short IdUsersRolUser { get; set; }
    }
}
