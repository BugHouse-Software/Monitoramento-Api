using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monitoramento.Service.Configure
{
    public static class AuthorizationConfiguration
    {

        public static void AddAllTokens(this IServiceCollection services)
        {
            Func<string> funcPass = () =>
            {
                var valuePassJson = File.ReadAllText(Path.Combine(".", "Pass.json"));

                var value = valuePassJson.ToDeserialize<PassDTO>();
                return value.CodeSecurity;

            };

            //services.AddTokenAdmin(funcPass);

            services.AddTokenBearer(JwtHelperService.GetHardDriveSerial());

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
               .AddJwtBearer("Admin", options =>
               {

                   options.Challenge = "Admin";
                   options.RequireHttpsMetadata = false;
                   options.SaveToken = true;
                   options.Events = new JwtBearerEvents
                   {
                       OnMessageReceived = context =>
                       {
                           var authorizationHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                           if (authorizationHeader != null && authorizationHeader.StartsWith("Admin "))
                           {
                               context.Token = authorizationHeader.Substring("Admin ".Length).Trim();
                           }
                           else
                           {
                               throw new AthorizationException("Invalid type token.");
                           }
                           return Task.CompletedTask;
                       }
                   };
                   options.TokenValidationParameters = ValidationParameters(funcPass, validateLifeTime: true);
               });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", policy =>
                {
                    policy.AuthenticationSchemes.Add("Admin");
                    policy.RequireAuthenticatedUser();

                });

                options.AddPolicy("Bearer", policy =>
                {
                    policy.AuthenticationSchemes.Add("Bearer");
                    policy.RequireAuthenticatedUser();
                });
            });
        }

        private static void AddTokenBearer(this IServiceCollection services, string key, bool validateLifeTime = true, bool validateAudience = false, bool validateIssuer = false, bool validateIssuerSigningKey = true)
        {
            if (services == null)
            {
                throw new ArgumentNullException("services");
            }

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = ValidationParameters(() => key, validateLifeTime, validateAudience, validateIssuer, validateIssuerSigningKey);
                });
        }

        private static void AddTokenAdmin(this IServiceCollection services, Func<string> token, bool validateLifeTime = true, bool validateAudience = false, bool validateIssuer = false, bool validateIssuerSigningKey = true)
        {
            if (services == null)
            {
                throw new ArgumentNullException("services");
            }

            services.AddAuthentication("Admin")
                .AddJwtBearer("Admin", options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = ValidationParameters(token, validateLifeTime, validateAudience, validateIssuer, validateIssuerSigningKey);
                });
        }


        internal static TokenValidationParameters ValidationParameters(Func<string> token,
                                                                       bool validateLifeTime = true,
                                                                       bool validateAudience = false,
                                                                       bool validateIssuer = false,
                                                                       bool validateIssuerSigningKey = true)
        {
            return new TokenValidationParameters
            {
                ValidateIssuerSigningKey = validateIssuerSigningKey,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(token.Invoke())),
                ValidateIssuer = validateIssuer,
                ValidateAudience = validateAudience,
                ValidateLifetime = validateLifeTime
            };
        }


    }
}
