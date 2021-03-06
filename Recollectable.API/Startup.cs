﻿using AspNetCoreRateLimit;
using AutoMapper;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using Recollectable.API.Filters;
using Recollectable.API.Interfaces;
using Recollectable.API.Services;
using Recollectable.API.Validators.Collectables;
using Recollectable.API.Validators.Collection;
using Recollectable.API.Validators.Location;
using Recollectable.API.Validators.Users;
using Recollectable.Core.Comparers;
using Recollectable.Core.Entities.Collectables;
using Recollectable.Core.Entities.Users;
using Recollectable.Core.Interfaces;
using Recollectable.Core.Services;
using Recollectable.Core.Shared.Entities;
using Recollectable.Core.Shared.Factories;
using Recollectable.Core.Shared.Interfaces;
using Recollectable.Core.Shared.Validators;
using Recollectable.Infrastructure.Data;
using Recollectable.Infrastructure.Email;
using Recollectable.Infrastructure.Interfaces;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Recollectable.API
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Setup Json Serializer
            services.AddControllers()
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver =
                    new CamelCasePropertyNamesContractResolver();
            });

            services.AddMvc(options => {
                options.ReturnHttpNotAcceptable = true;
                options.OutputFormatters.Add(new CustomXmlFormatter());
                options.InputFormatters.Add(new XmlSerializerInputFormatter(options));

                var jsonOutputFormatter = options.OutputFormatters
                    .OfType<NewtonsoftJsonOutputFormatter>().FirstOrDefault();

                if (jsonOutputFormatter != null)
                {
                    jsonOutputFormatter.SupportedMediaTypes.Add("application/json+hateoas");
                }

                //TODO Activate Authorization
                /*var policy = new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .Build();

                options.Filters.Add(new AuthorizeFilter(policy));*/
            })
            .AddFluentValidation(options => 
            {
                options.RegisterValidatorsFromAssemblyContaining<CoinCreationDtoValidator>();
                options.RegisterValidatorsFromAssemblyContaining<CollectionCreationDtoValidator>();
                options.RegisterValidatorsFromAssemblyContaining<CountryCreationDtoValidator>();
                options.RegisterValidatorsFromAssemblyContaining<UserCreationDtoValidator>();
            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            // Setup Configuration
            services.Configure<EmailConfiguration>(Configuration.GetSection("EmailConfiguration"));

            // Configure DbContext
            services.AddDbContext<RecollectableContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("RecollectableConnection")));

            // Configure User Identity
            services.AddIdentity<User, Role>(options =>
            {
                options.Tokens.EmailConfirmationTokenProvider = "email_confirmation";

                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredUniqueChars = 4;
                options.Password.RequiredLength = 8;

                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQ" +
                "RSTUVWXYZ0123456789@-._!àèìòùáéíóúäëïöüâêîôûãõßñç";

                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 25;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(120);
            })
            .AddEntityFrameworkStores<RecollectableContext>()
            .AddDefaultTokenProviders()
            .AddTokenProvider<EmailConfirmationTokenProvider<User>>("email_confirmation")
            .AddPasswordValidator<DoesNotContainPasswordValidator<User>>();
            services.Configure<DataProtectionTokenProviderOptions>(options =>
                options.TokenLifespan = TimeSpan.FromHours(3));
            services.Configure<EmailConfirmationTokenProviderOptions>(options =>
                options.TokenLifespan = TimeSpan.FromDays(2));
            services.Configure<PasswordHasherOptions>(options =>
            {
                options.IterationCount = 100000;
            });
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/api/users/login";
            });

            // Configure JWT Authentication
            var tokenProviderOptionsSection = Configuration.GetSection("JwtTokenProviderOptions");
            var tokenProviderOptions = tokenProviderOptionsSection.Get<JwtTokenProviderOptions>();
            services.Configure<JwtTokenProviderOptions>(tokenProviderOptionsSection);
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = tokenProviderOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = tokenProviderOptions.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = tokenProviderOptions.SecurityKey,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

            //TODO Activate CORS in .NET Core 3.0
            // Configure CORS Requests
            /*services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());
            });*/

            // Configure Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Configure Domain Services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ICoinService, CoinService>();
            services.AddScoped<IBanknoteService, BanknoteService>();
            services.AddScoped<ICollectionCollectableService, CollectionCollectableService>();
            services.AddScoped<ICollectorValueService, CollectorValueService>();
            services.AddScoped<IConditionService, ConditionService>();
            services.AddScoped<ICollectionService, CollectionService>();
            services.AddScoped<ICountryService, CountryService>();

            // Configure Helper Classes
            services.AddScoped<IRazorViewToStringRenderer, RazorViewToStringRenderer>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddSingleton<ITokenFactory, TokenFactory>();
            services.AddSingleton<IEmailService, EmailService>();

            // Configure Comparers
            services.AddSingleton<IEqualityComparer<Currency>, CurrencyComparer>();

            // Configure Auto Mapper
            var configuration = new MapperConfiguration(cfg =>
                cfg.AddProfile<RecollectableMappingProfile>());
            IMapper mapper = configuration.CreateMapper();
            services.AddSingleton(mapper);

            // Configure HTTP Caching
            services.AddResponseCaching();
            services.AddMemoryCache();

            // Configure Rate Limit
            services.Configure<IpRateLimitOptions>((options) =>
            {
                options.GeneralRules = new List<RateLimitRule>()
                {
                    new RateLimitRule()
                    {
                        Endpoint = "*",
                        Limit = 300,
                        Period = "900s"
                    }
                };
            });
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();

            //TODO Update Swagger Terms of Service
            // Configure Swagger
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Recollectable API",
                    Description = "Base API for collection management",
                    //TermsOfService = new Uri("None"),
                    Contact = new OpenApiContact
                    {
                        Name = "Mike Joosten",
                        Email = string.Empty,
                        Url = new Uri("https://www.linkedin.com/in/mike-joosten/")
                    }
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);

                xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name.Replace("API", "Core")}.xml";
                xmlPath = Path.Combine(AppContext.BaseDirectory.Replace("API", "Core"), xmlFile);
                options.IncludeXmlComments(xmlPath);

                //TODO Update Swagger Filter
                //options.DocumentFilter<HttpRequestsFilter>();
                //options.OperationFilter<FromHeaderAttributeFilter>();
            });

            // Configure Versioning
            services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ApiVersionReader = new HeaderApiVersionReader("api-version");
            });
        }

        public void Configure(IApplicationBuilder app, IHostEnvironment env,
            ILoggerFactory loggerFactory, RecollectableContext recollectableContext)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(appBuilder =>
                {
                    appBuilder.Run(async context =>
                    {
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("An unexpected server error occurred. " +
                            "Try again later.");
                    });
                });
            }

            recollectableContext.Database.Migrate();

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Recollectable API v1");
                options.RoutePrefix = string.Empty;
            });

            app.UseHttpsRedirection();
            app.UseIpRateLimiting();
            app.UseResponseCaching();

            app.Use(async (context, next) =>
            {
                context.Response.GetTypedHeaders().CacheControl =
                    new CacheControlHeaderValue()
                    {
                        Public = true,
                        NoCache = true
                    };

                context.Response.Headers[HeaderNames.Vary] =
                    new string[] { "Accept-Encoding" };

                await next();
            });

            //TODO Activate CORS in .NET Core 3.0
            //app.UseCors("CorsPolicy");

            app.UseRouting();
            app.UseEndpoints(builder => builder.MapControllers());
        }
    }
}