namespace CEA_API.Models.DTOs
{
    public class VerificationSuccessResponseDTO
    {
        public bool IsSuccess { get; set; }
        public string Token { get; set; }
        public int IdUser { get; set; }
        public int IdUsersRolUser { get; set; }
    }
}
