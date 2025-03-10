﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Microsoft.Owin.Security;
using Owin;
using System.IdentityModel.Tokens;
using System.Text;
using Microsoft.Owin.Security.Jwt;
using Microsoft.Owin.Security.OAuth;

namespace TodoListService
{
    public partial class Startup
    {
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];

        public void ConfigureAuth(IAppBuilder app)
        {
            // NOTE: The usual WindowsAzureActiveDirectoryBearerAuthentication middleware uses a
            // metadata endpoint which is not supported by the Microsoft identity platform endpoint.  Instead, this 
            // OpenIdConnectSecurityTokenProvider implementation can be used to fetch & use the OpenIdConnect
            // metadata document - which for the identity platform endpoint is https://login.microsoftonline.com/common/v2.0/.well-known/openid-configuration

            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions
            {
                AccessTokenFormat = new JwtFormat(
                    new TokenValidationParameters
                    {
                        // Check if the audience is intended to be this application
                        ValidAudiences = new [] { clientId, $"api://{clientId}" },

                        // Change below to 'true' if you want this Web API to accept tokens issued to one Azure AD tenant only (single-tenant)
                        // Note that this is a simplification for the quickstart here. You should validate the issuer. For details, 
                        // see https://github.com/Azure-Samples/active-directory-dotnet-native-aspnetcore
                        ValidateIssuer = false,

                    },
                    new OpenIdConnectSecurityTokenProvider("https://login.microsoftonline.com/common/v2.0/.well-known/openid-configuration")
                ),
            });
//            app.UseJwtBearerAuthentication(new JwtBearerAuthenticationOptions
//            {
//                //difference between? AllowedAudiences & TokenValidationParameters = new TokenValidationParameters { ValidAudience = "https://quickstarts/api",
//AllowedAudiences = new[] {""}
//            });

            app.UseJwtBearerAuthentication(new JwtBearerAuthenticationOptions
            {
                AuthenticationMode           = AuthenticationMode.Active,
                TokenValidationParameters    = new TokenValidationParameters
                {
                    // check in commitments. plug in these values...
                    //var apiKeySecret = CloudConfigurationManager.GetSetting("ApiTokenSecret");
                    //var apiIssuer = CloudConfigurationManager.GetSetting("ApiIssuer");
                    //var apiAudiences = CloudConfigurationManager.GetSetting("ApiAudiences").Split(' ');
                    
                    AuthenticationType = "Bearer",
                    ValidIssuer = "http://example.com",
                    ValidAudience = "doubleauthpoc",
                    IssuerSigningKey   = new InMemorySymmetricSecurityKey(Encoding.UTF8.GetBytes("secretsauceonitsownisnotlongenough")),
                }
            });
        }
    }
}
