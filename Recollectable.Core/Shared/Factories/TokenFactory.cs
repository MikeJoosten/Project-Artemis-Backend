﻿using Microsoft.Extensions.Options;
using Recollectable.Core.Shared.Entities;
using Recollectable.Core.Shared.Helpers;
using Recollectable.Core.Shared.Interfaces;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Recollectable.Core.Shared.Factories
{
    public class TokenFactory : ITokenFactory
    {
        private readonly JwtTokenProviderOptions _tokenProviderOptions;

        public TokenFactory(IOptions<JwtTokenProviderOptions> tokenProviderOptions)
        {
            _tokenProviderOptions = tokenProviderOptions?.Value;
        }

        public async Task<string> GenerateToken(string userName, ClaimsIdentity identity)
        {
            DateTime now = DateTime.UtcNow;

            identity.AddClaims(GetTokenClaims(userName));

            var token = new JwtSecurityToken(
                issuer: _tokenProviderOptions.Issuer,
                audience: _tokenProviderOptions.Audience,
                claims: identity.Claims,
                notBefore: now,
                expires: now.Add(JwtTokenProviderOptions.Expiration),
                signingCredentials: _tokenProviderOptions.SigningCredentials);

            var encodedToken = new JwtSecurityTokenHandler().WriteToken(token);

            return await Task.FromResult(encodedToken);
        }

        private Claim[] GetTokenClaims(string userName)
        {
            return new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateHelper.ToUnixEpochDate(_tokenProviderOptions.IssuedAt).ToString(),
                    ClaimValueTypes.Integer64)
            };
        }
    }
}