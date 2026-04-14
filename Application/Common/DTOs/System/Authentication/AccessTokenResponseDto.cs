namespace Application.Authentication.Commands.Login;

public class AccessTokenResponseDto
{
    public required string AccessToken { get; init; }

    public required int AccessTokenExpiresInMinutes { get; init; }

}
