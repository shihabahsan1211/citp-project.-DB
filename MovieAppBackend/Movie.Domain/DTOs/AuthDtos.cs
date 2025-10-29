namespace Movie.Domain.DTOs
{
    public record RegisterRequestDto(string Username, string Email, string Password);
    public record LoginRequestDto(string Username, string Password);
    public record AuthResponseDto(int UserId, string Username, string Token);
}
