namespace CEA_API.Models.DTOs
{
    public class PermissionDTO
    {
        public int IdPermission { get; set; }

        public string CodePermission { get; set; } = null!;

        public int IdSystemPermission { get; set; }

        public string NameSystem { get; set; }
    }
}
