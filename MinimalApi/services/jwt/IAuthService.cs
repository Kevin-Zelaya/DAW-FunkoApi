using CSharpFunctionalExtensions;

public interface IAuthService
{
    Task<Result<AuthResponse, DomainError>> LoginAsync(LoginRequest request);
    Task<Result<AuthResponse, DomainError>> SignupAsync(SingupRequest request);
}