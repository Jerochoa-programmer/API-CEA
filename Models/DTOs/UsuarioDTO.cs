namespace CEA_API.Models.DTOs
{
    public class UsuarioDTO
    {
        public int IdUser { get; set; }

        public string NameUser { get; set; } = null!;

        public string EmailUser { get; set; } = null!;

        public string? PassUser { get; set; }

        public short IdUsersRolUser { get; set; }

        public bool IsActive { get; set; }

        public DateTime? LastLogin { get; set; }

        public DateOnly? UnActiveDate { get; set; }
    }
}
