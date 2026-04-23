using System.Net;

using Application.Authentication;
using Application.Authentication.Commands.Login;

using FluentAssertions;

using Integration.Tests.Common;

using Presentation.Controllers;

using Xunit;

namespace Integration.Tests.System.Authentication;

public class AuthenticationControllerTests : TestBase
{
    public AuthenticationControllerTests(IntegrationContainerFixture fixture)
        : base(fixture)
    {
    }

    #region Login

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnAccessTokenAndSetCookie()
    {
        const string rawPassword = "TestPassword123!";

        var institute = await _instituteRepo.CreateInstituteAsync();
        var admin = await _userRepo.CreateAdminWithPasswordAsync(institute.Guid, rawPassword,
            email: "admin@test.com");

        var response = await _client.PostAsync(
            Routings.RestAuthenticationRouting + "login",
            BuildJsonContent(BuildLoginRequest(admin.Email, rawPassword)));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeResponseAsync<AccessTokenResponseDto>(response);

        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrEmpty();
        result.AccessTokenExpiresInMinutes.Should().BeGreaterThan(0);

        var setCookieHeader = response.Headers
            .SingleOrDefault(h => h.Key == "Set-Cookie")
            .Value?.FirstOrDefault();

        setCookieHeader.Should().NotBeNull();
        setCookieHeader.Should().Contain("RefreshToken=");
        setCookieHeader.Should().Contain("httponly");
        setCookieHeader.Should().Contain("secure");
        setCookieHeader.Should().Contain("samesite=strict");
    }

    [Fact]
    public async Task Login_WithWrongPassword_ShouldReturnUnauthorized()
    {
        const string rawPassword = "CorrectPassword123!";

        var institute = await _instituteRepo.CreateInstituteAsync();
        var admin = await _userRepo.CreateAdminWithPasswordAsync(institute.Guid, rawPassword,
            email: "admin@test.com");

        var response = await _client.PostAsync(
            Routings.RestAuthenticationRouting + "login",
            BuildJsonContent(BuildLoginRequest(admin.Email, "WrongPassword!")));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var error = await DeserializeResponseAsync<ErrorResponseDto>(response);

        error.Should().NotBeNull();
        error!.ErrorCode.Should().Be((int)AuthErrorCodes.WrongPassword);
    }

    [Fact]
    public async Task Login_WithUnknownUser_ShouldReturnUnauthorized()
    {
        var response = await _client.PostAsync(
            Routings.RestAuthenticationRouting + "login",
            BuildJsonContent(BuildLoginRequest("unknown@test.com", "SomePassword!")));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var error = await DeserializeResponseAsync<ErrorResponseDto>(response);

        error.Should().NotBeNull();
        error!.ErrorCode.Should().Be((int)AuthErrorCodes.NoUserWithThisName);
    }

    [Fact]
    public async Task Login_WithPendingUser_ShouldReturnUnauthorized()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var admin = await _userRepo.CreateAdminAsync(institute.Guid, email: "pending@test.com");

        var response = await _client.PostAsync(
            Routings.RestAuthenticationRouting + "login",
            BuildJsonContent(BuildLoginRequest(admin.Email, "AnyPassword!")));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region ValidateRefreshToken

    [Fact]
    public async Task ValidateRefreshToken_WithValidToken_ShouldReturnNewAccessToken()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var admin = await _userRepo.CreateAdminWithPasswordAsync(institute.Guid, "Password123!");

        var refreshToken = await _userRepo.CreateRefreshTokenForUserAsync(admin);

        _client.DefaultRequestHeaders.Add(
            "Cookie",
            $"RefreshToken={refreshToken.RefreshToken}");

        var response = await _client.GetAsync(Routings.RestAuthenticationRouting);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeResponseAsync<AccessTokenResponseDto>(response);

        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ValidateRefreshToken_WithMissingCookie_ShouldReturnUnauthorized()
    {
        var response = await _client.GetAsync(Routings.RestAuthenticationRouting);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var error = await DeserializeResponseAsync<ErrorResponseDto>(response);

        error.Should().NotBeNull();
        error!.ErrorCode.Should().Be((int)AuthErrorCodes.NoValidTokenFound);
    }

    [Fact]
    public async Task ValidateRefreshToken_WithInvalidGuidCookie_ShouldReturnUnauthorized()
    {
        _client.DefaultRequestHeaders.Add("Cookie", "RefreshToken=not-a-valid-guid");

        var response = await _client.GetAsync(Routings.RestAuthenticationRouting);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var error = await DeserializeResponseAsync<ErrorResponseDto>(response);

        error.Should().NotBeNull();
        error!.ErrorCode.Should().Be((int)AuthErrorCodes.NoValidTokenFound);
    }

    [Fact]
    public async Task ValidateRefreshToken_WithNonExistentToken_ShouldReturnUnauthorized()
    {
        _client.DefaultRequestHeaders.Add(
            "Cookie",
            $"RefreshToken={Guid.CreateVersion7()}");

        var response = await _client.GetAsync(Routings.RestAuthenticationRouting);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var error = await DeserializeResponseAsync<ErrorResponseDto>(response);

        error.Should().NotBeNull();
        error!.ErrorCode.Should().Be((int)AuthErrorCodes.NoUserWithThisToken);
    }

    #endregion

    #region Helpers

    private static LoginRequest BuildLoginRequest(
        string? loginName = null,
        string? password = null,
        string? deviceName = null,
        Guid? deviceGuid = null)
    {
        return new LoginRequest
        {
            LoginName = loginName ?? "default@test.com",
            Password = password ?? "DefaultPassword123!",
            DeviceName = deviceName ?? "Test Device",
            DeviceGuid = deviceGuid ?? Guid.CreateVersion7()
        };
    }

    #endregion
}
