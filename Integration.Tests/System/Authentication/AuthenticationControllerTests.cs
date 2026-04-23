using System.Net;
using System.Net.Http.Json;

using Application.Authentication;
using Application.Authentication.Commands.Login;
using Application.Common;

using Domain.Common.Enums;

using FluentAssertions;

using Integration.Tests.Common;

using Presentation.Controllers;

using Xunit;

using DomainUserErrors = Domain.Public.Users.UserErrorCodes;

namespace Integration.Tests.System.Authentication;

public class AuthenticationControllerTests : TestBase
{
    public AuthenticationControllerTests(IntegrationContainerFixture fixture)
        : base(fixture)
    {
    }

    #region Login – success

    [Fact]
    public async Task Login_WithValidEmail_ShouldReturnAccessTokenAndSetCookie()
    {
        const string rawPassword = "TestPassword123!";
        var institute = await _instituteRepo.CreateInstituteAsync();
        var admin = await _userRepo.CreateAdminAsync(institute.Guid,
            email: "admin@test.com",
            password: rawPassword);

        var request = new LoginRequest
        {
            LoginName = admin.Email,
            Password = rawPassword,
            DeviceName = "Test Device",
            DeviceGuid = Guid.CreateVersion7()
        };

        var response = await _client.PostAsJsonAsync(Routings.RestAuthenticationRouting + "login", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeResponseAsync<AccessTokenResponseDto>(response);
        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrEmpty();
        result.AccessTokenExpiresInMinutes.Should().BeGreaterThan(0);

        var cookie = response.Headers
            .SingleOrDefault(h => h.Key == "Set-Cookie").Value?.FirstOrDefault();
        cookie.Should().Contain("RefreshToken=");
        cookie.Should().Contain("httponly");
        cookie.Should().Contain("secure");
        cookie.Should().Contain("samesite=strict");
    }

    [Fact]
    public async Task Login_WithValidUsername_ShouldReturnAccessToken()
    {
        const string rawPassword = "TestPassword123!";
        var institute = await _instituteRepo.CreateInstituteAsync();
        await _userRepo.CreateAdminAsync(institute.Guid,
            username: "admin.testuser",
            password: rawPassword);

        var request = new LoginRequest
        {
            LoginName = "admin.testuser",
            Password = rawPassword,
            DeviceName = "Test Device",
            DeviceGuid = Guid.CreateVersion7()
        };

        var response = await _client.PostAsJsonAsync(Routings.RestAuthenticationRouting + "login", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Login – unknown user

    [Fact]
    public async Task Login_WithUnknownUser_ShouldReturnUnauthorized_WithNoUserWithThisName()
    {
        var request = new LoginRequest
        {
            LoginName = "unknown@test.com",
            Password = "AnyPassword!",
            DeviceName = "Test Device",
            DeviceGuid = Guid.CreateVersion7()
        };

        var response = await _client.PostAsJsonAsync(Routings.RestAuthenticationRouting + "login", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var error = await DeserializeResponseAsync<ErrorResponseDto>(response);
        error!.ErrorCode.Should().Be((int)AuthErrorCodes.NoUserWithThisName);
    }

    #endregion

    #region Login – wrong password

    [Fact]
    public async Task Login_WithWrongPassword_ShouldReturnUnauthorized_WithWrongPassword()
    {
        const string rawPassword = "CorrectPassword123!";
        var institute = await _instituteRepo.CreateInstituteAsync();
        var admin = await _userRepo.CreateAdminAsync(institute.Guid,
            email: "admin@test.com",
            password: rawPassword);

        var request = new LoginRequest
        {
            LoginName = admin.Email,
            Password = "WrongPassword!",
            DeviceName = "Test Device",
            DeviceGuid = Guid.CreateVersion7()
        };

        var response = await _client.PostAsJsonAsync(Routings.RestAuthenticationRouting + "login", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var error = await DeserializeResponseAsync<ErrorResponseDto>(response);
        error!.ErrorCode.Should().Be((int)AuthErrorCodes.WrongPassword);
    }

    #endregion

    #region Login – user state checks

    [Fact]
    public async Task Login_WithPendingUser_ShouldReturnBadRequest_WithUserIsNotActivated()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var user = await _userRepo.CreateAdminAsync(institute.Guid,
            email: "pending@test.com",
            state: States.Pending);

        var request = new LoginRequest
        {
            LoginName = user.Email,
            Password = "AnyPassword!",
            DeviceName = "Test Device",
            DeviceGuid = Guid.CreateVersion7()
        };

        var response = await _client.PostAsJsonAsync(Routings.RestAuthenticationRouting + "login", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await DeserializeResponseAsync<ErrorResponseDto>(response);
        error!.ErrorCode.Should().Be((int)DomainUserErrors.UserIsNotActivated);
    }

    [Fact]
    public async Task Login_WithDeactivatedUser_ShouldReturnBadRequest_WithUserIsDeactivated()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var user = await _userRepo.CreateAdminAsync(institute.Guid,
            email: "deactivated@test.com",
            state: States.Deactived);

        var request = new LoginRequest
        {
            LoginName = user.Email,
            Password = "AnyPassword!",
            DeviceName = "Test Device",
            DeviceGuid = Guid.CreateVersion7()
        };

        var response = await _client.PostAsJsonAsync(Routings.RestAuthenticationRouting + "login", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await DeserializeResponseAsync<ErrorResponseDto>(response);
        error!.ErrorCode.Should().Be((int)DomainUserErrors.UserIsDeactivated);
    }

    [Fact]
    public async Task Login_WithDeletedUser_ShouldReturnBadRequest_WithUserIsDeleted()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var user = await _userRepo.CreateAdminAsync(institute.Guid,
            email: "deleted@test.com",
            state: States.Deleted);

        var request = new LoginRequest
        {
            LoginName = user.Email,
            Password = "AnyPassword!",
            DeviceName = "Test Device",
            DeviceGuid = Guid.CreateVersion7()
        };

        var response = await _client.PostAsJsonAsync(Routings.RestAuthenticationRouting + "login", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await DeserializeResponseAsync<ErrorResponseDto>(response);
        error!.ErrorCode.Should().Be((int)DomainUserErrors.UserIsDeleted);
    }

    [Fact]
    public async Task Login_WithBlockedUser_ShouldReturnBadRequest_WithUserIsBlocked()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var user = await _userRepo.CreateAdminAsync(institute.Guid,
            email: "blocked@test.com",
            isBlocked: true);

        var request = new LoginRequest
        {
            LoginName = user.Email,
            Password = "AnyPassword!",
            DeviceName = "Test Device",
            DeviceGuid = Guid.CreateVersion7()
        };

        var response = await _client.PostAsJsonAsync(Routings.RestAuthenticationRouting + "login", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await DeserializeResponseAsync<ErrorResponseDto>(response);
        error!.ErrorCode.Should().Be((int)DomainUserErrors.UserIsBlocked);
    }

    [Fact]
    public async Task Login_WithUserWithNoPassword_ShouldReturnInternalServerError()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var user = await _userRepo.CreateAdminAsync(institute.Guid, email: "nopassword@test.com");

        var request = new LoginRequest
        {
            LoginName = user.Email,
            Password = "AnyPassword!",
            DeviceName = "Test Device",
            DeviceGuid = Guid.CreateVersion7()
        };

        var response = await _client.PostAsJsonAsync(Routings.RestAuthenticationRouting + "login", request);

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        var error = await DeserializeResponseAsync<ErrorResponseDto>(response);
        error!.ErrorCode.Should().Be((int)CommonErrorCodes.DefaultErrorCode);
    }

    #endregion

    #region Login – login attempt blocking

    [Fact]
    public async Task Login_AfterMaxWrongAttempts_ShouldBlockUserOnNextLogin()
    {
        const string rawPassword = "CorrectPassword123!";
        var institute = await _instituteRepo.CreateInstituteAsync();
        var admin = await _userRepo.CreateAdminAsync(institute.Guid,
            email: "block-me@test.com",
            password: rawPassword);

        for (var i = 0; i < 5; i++)
        {
            var wrongRequest = new LoginRequest
            {
                LoginName = admin.Email,
                Password = "WrongPassword!",
                DeviceName = "Test Device",
                DeviceGuid = Guid.CreateVersion7()
            };

            await _client.PostAsJsonAsync(Routings.RestAuthenticationRouting + "login", wrongRequest);
        }

        var request = new LoginRequest
        {
            LoginName = admin.Email,
            Password = rawPassword,
            DeviceName = "Test Device",
            DeviceGuid = Guid.CreateVersion7()
        };

        var response = await _client.PostAsJsonAsync(Routings.RestAuthenticationRouting + "login", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await DeserializeResponseAsync<ErrorResponseDto>(response);
        error!.ErrorCode.Should().Be((int)DomainUserErrors.UserIsBlocked);
    }

    #endregion

    #region ValidateRefreshToken – success

    [Fact]
    public async Task ValidateRefreshToken_WithValidToken_ShouldReturnNewAccessToken()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var admin = await _userRepo.CreateAdminAsync(institute.Guid, password: "Password123!");
        var token = await _userRepo.CreateRefreshTokenForUserAsync(admin);

        _client.DefaultRequestHeaders.Add("Cookie", $"RefreshToken={token.RefreshToken}");

        var response = await _client.GetAsync(Routings.RestAuthenticationRouting);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeResponseAsync<AccessTokenResponseDto>(response);
        result!.AccessToken.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region ValidateRefreshToken – cookie validation

    [Fact]
    public async Task ValidateRefreshToken_WithMissingCookie_ShouldReturnUnauthorized_WithNoValidTokenFound()
    {
        var response = await _client.GetAsync(Routings.RestAuthenticationRouting);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var error = await DeserializeResponseAsync<ErrorResponseDto>(response);
        error!.ErrorCode.Should().Be((int)AuthErrorCodes.NoValidTokenFound);
    }

    [Fact]
    public async Task ValidateRefreshToken_WithInvalidGuidCookie_ShouldReturnUnauthorized_WithNoValidTokenFound()
    {
        _client.DefaultRequestHeaders.Add("Cookie", "RefreshToken=not-a-valid-guid");

        var response = await _client.GetAsync(Routings.RestAuthenticationRouting);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var error = await DeserializeResponseAsync<ErrorResponseDto>(response);
        error!.ErrorCode.Should().Be((int)AuthErrorCodes.NoValidTokenFound);
    }

    [Fact]
    public async Task ValidateRefreshToken_WithNonExistentToken_ShouldReturnUnauthorized_WithNoUserWithThisToken()
    {
        _client.DefaultRequestHeaders.Add("Cookie", $"RefreshToken={Guid.CreateVersion7()}");

        var response = await _client.GetAsync(Routings.RestAuthenticationRouting);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var error = await DeserializeResponseAsync<ErrorResponseDto>(response);
        error!.ErrorCode.Should().Be((int)AuthErrorCodes.NoUserWithThisToken);
    }

    #endregion

    #region ValidateRefreshToken – token state

    [Fact]
    public async Task ValidateRefreshToken_WithExpiredToken_ShouldReturnUnauthorized_WithNoValidTokenFound()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var admin = await _userRepo.CreateAdminAsync(institute.Guid, password: "Password123!");
        var expiredToken = await _userRepo.CreateExpiredRefreshTokenForUserAsync(admin);

        _client.DefaultRequestHeaders.Add("Cookie", $"RefreshToken={expiredToken.RefreshToken}");

        var response = await _client.GetAsync(Routings.RestAuthenticationRouting);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var error = await DeserializeResponseAsync<ErrorResponseDto>(response);
        error!.ErrorCode.Should().Be((int)AuthErrorCodes.NoValidTokenFound);
    }

    [Fact]
    public async Task ValidateRefreshToken_WithRevokedToken_ShouldReturnUnauthorized_WithNoValidTokenFound()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var admin = await _userRepo.CreateAdminAsync(institute.Guid, password: "Password123!");
        var revokedToken = await _userRepo.CreateRevokedRefreshTokenForUserAsync(admin);

        _client.DefaultRequestHeaders.Add("Cookie", $"RefreshToken={revokedToken.RefreshToken}");

        var response = await _client.GetAsync(Routings.RestAuthenticationRouting);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var error = await DeserializeResponseAsync<ErrorResponseDto>(response);
        error!.ErrorCode.Should().Be((int)AuthErrorCodes.NoValidTokenFound);
    }

    #endregion

    #region ValidateRefreshToken – user state checks

    [Fact]
    public async Task ValidateRefreshToken_WithBlockedUser_ShouldReturnBadRequest_WithUserIsBlocked()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var user = await _userRepo.CreateAdminAsync(institute.Guid, isBlocked: true);
        var token = await _userRepo.CreateRefreshTokenForUserAsync(user);

        _client.DefaultRequestHeaders.Add("Cookie", $"RefreshToken={token.RefreshToken}");

        var response = await _client.GetAsync(Routings.RestAuthenticationRouting);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await DeserializeResponseAsync<ErrorResponseDto>(response);
        error!.ErrorCode.Should().Be((int)DomainUserErrors.UserIsBlocked);
    }

    [Fact]
    public async Task ValidateRefreshToken_WithPendingUser_ShouldReturnBadRequest_WithUserIsNotActivated()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var user = await _userRepo.CreateAdminAsync(institute.Guid, state: States.Pending);
        var token = await _userRepo.CreateRefreshTokenForUserAsync(user);

        _client.DefaultRequestHeaders.Add("Cookie", $"RefreshToken={token.RefreshToken}");

        var response = await _client.GetAsync(Routings.RestAuthenticationRouting);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await DeserializeResponseAsync<ErrorResponseDto>(response);
        error!.ErrorCode.Should().Be((int)DomainUserErrors.UserIsNotActivated);
    }

    [Fact]
    public async Task ValidateRefreshToken_WithDeactivatedUser_ShouldReturnBadRequest_WithUserIsDeactivated()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var user = await _userRepo.CreateAdminAsync(institute.Guid, state: States.Deactived);
        var token = await _userRepo.CreateRefreshTokenForUserAsync(user);

        _client.DefaultRequestHeaders.Add("Cookie", $"RefreshToken={token.RefreshToken}");

        var response = await _client.GetAsync(Routings.RestAuthenticationRouting);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await DeserializeResponseAsync<ErrorResponseDto>(response);
        error!.ErrorCode.Should().Be((int)DomainUserErrors.UserIsDeactivated);
    }

    [Fact]
    public async Task ValidateRefreshToken_WithDeletedUser_ShouldReturnBadRequest_WithUserIsDeleted()
    {
        var institute = await _instituteRepo.CreateInstituteAsync();
        var user = await _userRepo.CreateAdminAsync(institute.Guid, state: States.Deleted);
        var token = await _userRepo.CreateRefreshTokenForUserAsync(user);

        _client.DefaultRequestHeaders.Add("Cookie", $"RefreshToken={token.RefreshToken}");

        var response = await _client.GetAsync(Routings.RestAuthenticationRouting);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await DeserializeResponseAsync<ErrorResponseDto>(response);
        error!.ErrorCode.Should().Be((int)DomainUserErrors.UserIsDeleted);
    }

    #endregion
}
