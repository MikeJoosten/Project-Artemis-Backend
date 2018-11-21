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
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Serialization;
using Recollectable.API.Filters;
using Recollectable.API.Validators.Collectables;
using Recollectable.API.Validators.Collection;
using Recollectable.API.Validators.Location;
using Recollectable.API.Validators.Users;
using Recollectable.Core.Comparers;
using Recollectable.Core.Entities.Collectables;
using Recollectable.Core.Entities.Collections;
using Recollectable.Core.Entities.Locations;
using Recollectable.Core.Entities.Users;
using Recollectable.Core.Interfaces;
using Recollectable.Core.Services;
using Recollectable.Core.Shared.Entities;
using Recollectable.Core.Shared.Factories;
using Recollectable.Core.Shared.Interfaces;
using Recollectable.Core.Shared.Validators;
using Recollectable.Infrastructure.Data;
using Recollectable.Infrastructure.Data.Repositories;
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

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options => {
                options.ReturnHttpNotAcceptable = true;
                options.OutputFormatters.Add(new XmlSerializerOutputFormatter());
                options.InputFormatters.Add(new XmlSerializerInputFormatter(options));

                var jsonOutputFormatter = options.OutputFormatters
                    .OfType<JsonOutputFormatter>().FirstOrDefault();

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
            .AddJsonOptions(options =>
            {
                options.SerializerSettings.ContractResolver =
                    new CamelCasePropertyNamesContractResolver();
            })
            .AddFluentValidation(options => 
            {
                options.RegisterValidatorsFromAssemblyContaining<CollectableCreationDtoValidator>();
                options.RegisterValidatorsFromAssemblyContaining<CollectionCreationDtoValidator>();
                options.RegisterValidatorsFromAssemblyContaining<CountryCreationDtoValidator>();
                options.RegisterValidatorsFromAssemblyContaining<UserCreationDtoValidator>();
            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // Configure DbContext
            services.AddDbContext<RecollectableContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("RecollectableConnection")));

            // Configure User Identity
            services.AddIdentity<User, Role>(options =>
            {
                options.Tokens.EmailConfirmationTokenProvider = "email_confirmation";

                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredUniqueChars = 4;
                options.Password.RequiredLength = 8;

                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._!" +
                    "àèìòùáéíóúäëïöüâêîôûãõßñç";

                //TODO Improve Lockout features
                /*options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 35;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);*/
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

            // Configure CORS Requests
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());
            });

            // Configure Repositories
            services.AddScoped<IRepository<User>, UserRepository>();
            services.AddScoped<IRepository<Coin>, CoinRepository>();
            services.AddScoped<IRepository<Banknote>, BanknoteRepository>();
            services.AddScoped<IRepository<Collectable>, CollectableRepository>();
            services.AddScoped<IRepository<CollectionCollectable>, CollectionCollectableRepository>();
            services.AddScoped<IRepository<CollectorValue>, CollectorValueRepository>();
            services.AddScoped<IRepository<Collection>, CollectionRepository>();
            services.AddScoped<IRepository<Country>, CountryRepository>();

            // Configure Domain Services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ICoinService, CoinService>();
            services.AddScoped<IBanknoteService, BanknoteService>();
            services.AddScoped<ICollectionCollectableService, CollectionCollectableService>();
            services.AddScoped<ICollectorValueService, CollectorValueService>();
            services.AddScoped<ICollectionService, CollectionService>();
            services.AddScoped<ICountryService, CountryService>();

            // Configure Helper Classes
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddSingleton<ITokenFactory, TokenFactory>();
            services.AddSingleton<IEmailService, EmailService>();

            //Configure Comparers
            services.AddSingleton<IEqualityComparer<Currency>, CurrencyComparer>();

            // Configure Auto Mapper
            var configuration = new MapperConfiguration(cfg =>
                cfg.AddProfile<RecollectableMappingProfile>());
            IMapper mapper = configuration.CreateMapper();
            services.AddSingleton(mapper);

            // Configure HTTP Caching
            services.AddHttpCacheHeaders(
                (expirationModelOptions) => 
                {
                    expirationModelOptions.MaxAge = 1;
                },
                (validationModelOptions) =>
                {
                    validationModelOptions.MustRevalidate = true;
                });
            services.AddResponseCaching();
            services.AddMemoryCache();
            services.Configure<IpRateLimitOptions>((options) =>
            {
                options.GeneralRules = new List<RateLimitRule>()
                {
                    new RateLimitRule()
                    {
                        Endpoint = "*",
                        Limit = 1000000000,
                        Period = "1s"
                    }
                };
            });
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();

            // Configure Swagger
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Info
                {
                    Version = "v1",
                    Title = "Recollectable API",
                    Description = "Base API for collection management",
                    TermsOfService = "None",
                    Contact = new Contact
                    {
                        Name = "Mike Joosten",
                        Email = string.Empty,
                        Url = "https://www.linkedin.com/in/mike-joosten/"
                    }
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);

                xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name.Replace("API", "Core")}.xml";
                xmlPath = Path.Combine(AppContext.BaseDirectory.Replace("API", "Core"), xmlFile);
                options.IncludeXmlComments(xmlPath);

                options.DocumentFilter<HttpRequestsFilter>();
                options.OperationFilter<FromHeaderAttributeFilter>();
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

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env,
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

            Mapper.Initialize(cfg =>
                cfg.AddProfile<RecollectableMappingProfile>());

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
            app.UseHttpCacheHeaders();
            app.UseCors("CorsPolicy");
            app.UseMvc();
        }
    }
}