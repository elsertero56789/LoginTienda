using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Frontend.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity()); //usuario no authenticado
            return Task.FromResult(new AuthenticationState(user));
        }
    }
}
