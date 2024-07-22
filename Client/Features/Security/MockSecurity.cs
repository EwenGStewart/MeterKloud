using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace MeterKloud.Features.Security
{


    public class MockAuthenticatedUser : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private const string userId = "phv";
        private const string userName = "Jean Irvine";
        private const string userRole = "ProductManager";

        public MockAuthenticatedUser(
          IOptionsMonitor<AuthenticationSchemeOptions> options,
          ILoggerFactory logger,
          UrlEncoder encoder,
          ISystemClock clock)
          : base(options, logger, encoder, clock) { }


        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[]
              {
          new Claim(ClaimTypes.NameIdentifier, userId),
          new Claim(ClaimTypes.Name, userName),
          new Claim(ClaimTypes.Role, userRole),
          new Claim(ClaimTypes.Email, "peter.vogel@phvis.com"),
        };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return await Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }



    public class MyServerAuthenticationStateProvider : AuthenticationStateProvider
    {

        private const string userId = "phv";
        private const string userName = "Jean Irvine";
        private const string userRole = "ProductManager";

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var claims = new[] {
          new Claim(ClaimTypes.NameIdentifier, userId),
          new Claim(ClaimTypes.Name, userName),
          new Claim(ClaimTypes.Role, userRole),
          new Claim(ClaimTypes.Email, "peter.vogel@phvis.com"),
        };
            var identity = new ClaimsIdentity(claims, "BasicAuthentication");
            //    var authenticated = true;

            var result =await Task.FromResult( new AuthenticationState(new ClaimsPrincipal(identity)));
            return result;
        }

    }


}

