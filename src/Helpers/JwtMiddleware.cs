﻿using Meetup.Services.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Meetup
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly AppSettings _appSettings;

        public JwtMiddleware(RequestDelegate next, IOptions<AppSettings> appSettings)
        {
            _next = next;
            _appSettings = appSettings.Value;
        }

        public async Task Invoke(HttpContext context, IUserService userService)
        {

            var token = context.Request.Cookies["Authorization"];
            if (token != null)
                attachUserToContext(context, userService, token);
            await _next(context);
        }

        private void attachUserToContext(HttpContext context, IUserService userService, string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);
                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = (jwtToken.Claims.First(x => x.Type == "Id").Value);
                context.Items["User"] = userService.GetById(userId);
            }
            catch
            {
            }
        }
    }
}
