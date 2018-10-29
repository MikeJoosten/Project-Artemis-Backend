﻿using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Recollectable.API.Interfaces;
using Recollectable.Core.Entities.ResourceParameters;
using Recollectable.Core.Entities.Users;
using Recollectable.Core.Interfaces;
using Recollectable.Core.Models.Users;
using Recollectable.Core.Shared.Entities;
using Recollectable.Core.Shared.Enums;
using Recollectable.Core.Shared.Extensions;
using Recollectable.Core.Shared.Interfaces;
using Recollectable.Core.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Recollectable.API.Controllers
{
    [Route("api/users")]
    //TODO Add Authorization [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private IUnitOfWork _unitOfWork;
        private IPropertyMappingService _propertyMappingService;
        private ITypeHelperService _typeHelperService;
        private UserManager<User> _userManager;
        private ITokenFactory _tokenFactory;
        private IEmailService _emailService;
        private IMapper _mapper;

        public UsersController(IUnitOfWork unitOfWork, ITypeHelperService typeHelperService,
            IPropertyMappingService propertyMappingService, UserManager<User> userManager, 
            ITokenFactory tokenFactory, IEmailService emailService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _propertyMappingService = propertyMappingService;
            _typeHelperService = typeHelperService;
            _userManager = userManager;
            _tokenFactory = tokenFactory;
            _emailService = emailService;
            _mapper = mapper;
        }

        [HttpHead]
        [HttpGet(Name = "GetUsers")]
        public IActionResult GetUsers(UsersResourceParameters resourceParameters,
            [FromHeader(Name = "Accept")] string mediaType)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<UserDto, User>
                (resourceParameters.OrderBy))
            {
                return BadRequest();
            }

            if (!_typeHelperService.TypeHasProperties<UserDto>
                (resourceParameters.Fields))
            {
                return BadRequest();
            }

            var usersFromRepo = _unitOfWork.UserRepository.Get(resourceParameters);
            var users = _mapper.Map<IEnumerable<UserDto>>(usersFromRepo);

            if (mediaType == "application/json+hateoas")
            {
                var paginationMetadata = new
                {
                    totalCount = usersFromRepo.TotalCount,
                    pageSize = usersFromRepo.PageSize,
                    currentPage = usersFromRepo.CurrentPage,
                    totalPages = usersFromRepo.TotalPages
                };

                Response.Headers.Add("X-Pagination",
                    JsonConvert.SerializeObject(paginationMetadata));

                var links = CreateUsersLinks(resourceParameters,
                    usersFromRepo.HasNext, usersFromRepo.HasPrevious);
                var shapedUsers = users.ShapeData(resourceParameters.Fields);

                var linkedUsers = shapedUsers.Select(user =>
                {
                    var userAsDictionary = user as IDictionary<string, object>;
                    var userLinks = CreateUserLinks((Guid)userAsDictionary["Id"],
                        resourceParameters.Fields);

                    userAsDictionary.Add("links", userLinks);

                    return userAsDictionary;
                });

                var linkedCollectionResource = new LinkedCollectionResource
                {
                    Value = linkedUsers,
                    Links = links
                };

                return Ok(linkedCollectionResource);
            }
            else if (mediaType == "application/json")
            {
                var previousPageLink = usersFromRepo.HasPrevious ?
                    CreateUsersResourceUri(resourceParameters,
                    ResourceUriType.PreviousPage) : null;

                var nextPageLink = usersFromRepo.HasNext ?
                    CreateUsersResourceUri(resourceParameters,
                    ResourceUriType.NextPage) : null;

                var paginationMetadata = new
                {
                    totalCount = usersFromRepo.TotalCount,
                    pageSize = usersFromRepo.PageSize,
                    currentPage = usersFromRepo.CurrentPage,
                    totalPages = usersFromRepo.TotalPages,
                    previousPageLink,
                    nextPageLink,
                };

                Response.Headers.Add("X-Pagination",
                    JsonConvert.SerializeObject(paginationMetadata));

                return Ok(users.ShapeData(resourceParameters.Fields));
            }
            else
            {
                return Ok(users);
            }
        }

        [HttpGet("{id}", Name = "GetUser")]
        public IActionResult GetUser(Guid id, [FromQuery] string fields,
            [FromHeader(Name = "Accept")] string mediaType)
        {
            if (!_typeHelperService.TypeHasProperties<UserDto>(fields))
            {
                return BadRequest();
            }

            var userFromRepo = _unitOfWork.UserRepository.GetById(id);

            if (userFromRepo == null)
            {
                return NotFound();
            }

            var user = _mapper.Map<UserDto>(userFromRepo);

            if (mediaType == "application/json+hateoas")
            {
                var links = CreateUserLinks(id, fields);
                var linkedResource = user.ShapeData(fields)
                    as IDictionary<string, object>;

                linkedResource.Add("links", links);

                return Ok(linkedResource);
            }
            else if (mediaType == "application/json")
            {
                return Ok(user.ShapeData(fields));
            }
            else
            {
                return Ok(user);
            }
        }

        [AllowAnonymous]
        [HttpPost("register", Name = "Register")]
        public IActionResult Register([FromBody] UserCreationDto user,
            [FromHeader(Name = "Accept")] string mediaType)
        {
            if (user == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }

            var newUser = _mapper.Map<User>(user);
            var result = _userManager.CreateAsync(newUser, user.Password);

            if (!result.Result.Succeeded)
            {
                return new UnprocessableEntityObjectResult(result.Result);
            }

            result = _userManager.AddToRoleAsync(newUser, nameof(Roles.User));

            if (!result.Result.Succeeded)
            {
                return new UnprocessableEntityObjectResult(result.Result);
            }

            if (!_unitOfWork.Save())
            {
                throw new Exception("Creating a user failed on save.");
            }

            var token = _userManager.GenerateEmailConfirmationTokenAsync(newUser).Result;
            var confirmationUrl = Url.Action("ConfirmEmail", "Users", new { token, email = newUser.Email },
                protocol: HttpContext.Request.Scheme);

            //TODO Activate Mailing Service
            //_emailService.Send("Recipient's Email", "Confirmation", confirmationurl);

            var returnedUser = _mapper.Map<UserDto>(newUser);

            if (mediaType == "application/json+hateoas")
            {
                var links = CreateUserLinks(returnedUser.Id, null);
                var linkedResource = returnedUser.ShapeData(null)
                    as IDictionary<string, object>;

                linkedResource.Add("links", links);

                return CreatedAtRoute("GetUser", new { id = returnedUser.Id }, linkedResource);
            }
            else
            {
                return CreatedAtRoute("GetUser", new { id = returnedUser.Id }, returnedUser);
            }
        }

        [HttpPost("register/{id}")]
        public IActionResult BlockRegistration(Guid id)
        {
            if (_unitOfWork.UserRepository.Exists(id))
            {
                return new StatusCodeResult(StatusCodes.Status409Conflict);
            }

            return NotFound();
        }

        [AllowAnonymous]
        [HttpPost("login", Name = "Login")]
        public IActionResult Login([FromBody] CredentialsDto credentials)
        {
            if (credentials == null)
            {
                return BadRequest();
            }

            var user = _userManager.FindByNameOrEmailAsync(credentials.UserName).Result;

            if (user == null)
            {
                return NotFound();
            }

            var identity = GenerateClaimsIdentity(user, credentials.Password).Result;

            if (identity == null)
            {
                return BadRequest(ModelState);
            }

            var response = new
            {
                userName = user.UserName,
                roles = _userManager.GetRolesAsync(user).Result,
                auth_token = _tokenFactory.GenerateToken(credentials.UserName, identity).Result,
                expires_in = (int)JwtTokenProviderOptions.Expiration.TotalSeconds
            };

            HttpContext.SignInAsync("Identity.Application", new ClaimsPrincipal(identity));
            return Ok(response);
        }

        [AllowAnonymous]
        [HttpGet("{email}/forgot_password")]
        public IActionResult ForgotPassword(string email)
        {
            var user = _userManager.FindByEmailAsync(email).Result;

            if (user == null)
            {
                return NotFound();
            }

            var token = _userManager.GeneratePasswordResetTokenAsync(user).Result;
            var resetUrl = Url.Action("ResetPassword", "Users", new { token, email },
                protocol: HttpContext.Request.Scheme);

            //TODO Activate Mailing Service
            //_emailService.Send("Recipient's Email", "Reset Password", resetUrl);

            return NoContent();
        }

        [AllowAnonymous]
        [HttpPost("/{email}/reset_password")]
        public IActionResult ResetPassword(string token, string email, [FromBody] ResetPasswordDto resetPassword)
        {
            if (resetPassword == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }

            var user = _userManager.FindByEmailAsync(email).Result;

            if (user == null)
            {
                return NotFound();
            }

            var result = _userManager.ResetPasswordAsync(user, token, resetPassword.Password).Result;

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("Error", error.Description);
                }
                return BadRequest(ModelState);
            }

            if (_userManager.IsLockedOutAsync(user).Result)
            {
                _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);
            }

            return NoContent();
        }

        [Authorize(Roles = "User")]
        [HttpPost("/{email}/change_password")]
        public IActionResult ChangePassword(string email, [FromBody] ChangedPasswordDto changedPassword)
        {
            if (changedPassword == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }

            var user = _userManager.FindByEmailAsync(email).Result;

            if (user == null)
            {
                return NotFound();
            }

            var result = _userManager.ChangePasswordAsync(user, changedPassword.OldPassword, changedPassword.NewPassword).Result;

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("Error", error.Description);
                }
                return BadRequest(ModelState);
            }

            return NoContent();
        }

        [AllowAnonymous]
        [HttpGet("{email}/confirm_account")]
        public IActionResult ConfirmEmail(string token, string email)
        {
            var user = _userManager.FindByEmailAsync(email).Result;

            if (user == null)
            {
                return NotFound();
            }

            var result = _userManager.ConfirmEmailAsync(user, token).Result;
            return NoContent();
        }

        [HttpPut("{id}", Name = "UpdateUser")]
        public IActionResult UpdateUser(Guid id, [FromBody] UserUpdateDto user)
        {
            if (user == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }

            var userFromRepo = _unitOfWork.UserRepository.GetById(id);

            if (userFromRepo == null)
            {
                return NotFound();
            }

            _mapper.Map(user, userFromRepo);
            _unitOfWork.UserRepository.Update(userFromRepo);

            if (!_unitOfWork.Save())
            {
                throw new Exception($"Updating user {id} failed on save.");
            }

            return NoContent();
        }

        [HttpPatch("{id}", Name = "PartiallyUpdateUser")]
        public IActionResult PartiallyUpdateUser(Guid id,
            [FromBody] JsonPatchDocument<UserUpdateDto> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }

            var userFromRepo = _unitOfWork.UserRepository.GetById(id);

            if (userFromRepo == null)
            {
                return NotFound();
            }

            var patchedUser = _mapper.Map<UserUpdateDto>(userFromRepo);
            patchDoc.ApplyTo(patchedUser, ModelState);

            TryValidateModel(patchedUser);

            if (!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }

            _mapper.Map(patchedUser, userFromRepo);
            _unitOfWork.UserRepository.Update(userFromRepo);

            if (!_unitOfWork.Save())
            {
                throw new Exception($"Patching user {id} failed on save.");
            }

            return NoContent();
        }

        [HttpDelete("{id}", Name = "DeleteUser")]
        public IActionResult DeleteUser(Guid id)
        {
            var userFromRepo = _unitOfWork.UserRepository.GetById(id);

            if (userFromRepo == null)
            {
                return NotFound();
            }

            _unitOfWork.UserRepository.Delete(userFromRepo);

            if (!_unitOfWork.Save())
            {
                throw new Exception($"Deleting user {id} failed on save.");
            }

            return NoContent();
        }

        [HttpOptions]
        public IActionResult GetUsersOptions()
        {
            Response.Headers.Add("Allow", "GET - OPTIONS - POST - PUT - PATCH - DELETE");
            return Ok();
        }

        private async Task<ClaimsIdentity> GenerateClaimsIdentity(User user, string password)
        {
            if (string.IsNullOrEmpty(user.UserName))
            {
                ModelState.AddModelError("Error", "Invalid username");
                return await Task.FromResult<ClaimsIdentity>(null);
            }

            if (string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("Error", "Invalid password");
                return await Task.FromResult<ClaimsIdentity>(null);
            }

            if (!await _userManager.IsLockedOutAsync(user))
            {
                if (await _userManager.CheckPasswordAsync(user, password))
                {
                    if (!await _userManager.IsEmailConfirmedAsync(user))
                    {
                        ModelState.AddModelError("Error", "Email is not confirmed");
                        return await Task.FromResult<ClaimsIdentity>(null);
                    }

                    await _userManager.ResetAccessFailedCountAsync(user);

                    var roles = await _userManager.GetRolesAsync(user);
                    var identity = new ClaimsIdentity("Identity.Application");
                    identity.AddClaims(roles.Select(role => new Claim(ClaimTypes.Role, role)));

                    return await Task.FromResult(identity);
                }

                await _userManager.AccessFailedAsync(user);
            }

            if (await _userManager.IsLockedOutAsync(user))
            {
                ModelState.AddModelError("Error", "User locked out");
                //TODO Notify user + Send email password reset
            }
            else
            {
                ModelState.AddModelError("Error", "Invalid password");
            }

            return await Task.FromResult<ClaimsIdentity>(null);
        }

        private string CreateUsersResourceUri(UsersResourceParameters resourceParameters,
            ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return Url.Link("GetUsers", new
                    {
                        search = resourceParameters.Search,
                        orderBy = resourceParameters.OrderBy,
                        fields = resourceParameters.Fields,
                        page = resourceParameters.Page - 1,
                        pageSize = resourceParameters.PageSize
                    });
                case ResourceUriType.NextPage:
                    return Url.Link("GetUsers", new
                    {
                        search = resourceParameters.Search,
                        orderBy = resourceParameters.OrderBy,
                        fields = resourceParameters.Fields,
                        page = resourceParameters.Page + 1,
                        pageSize = resourceParameters.PageSize
                    });
                case ResourceUriType.Current:
                default:
                    return Url.Link("GetUsers", new
                    {
                        search = resourceParameters.Search,
                        orderBy = resourceParameters.OrderBy,
                        fields = resourceParameters.Fields,
                        page = resourceParameters.Page,
                        pageSize = resourceParameters.PageSize
                    });
            }
        }

        private IEnumerable<LinkDto> CreateUserLinks(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrEmpty(fields))
            {
                links.Add(new LinkDto(Url.Link("GetUser",
                    new { id }), "self", "GET"));

                links.Add(new LinkDto(Url.Link("Register",
                    new { }), "register_user", "POST"));

                links.Add(new LinkDto(Url.Link("Login",
                    new { }), "login_user", "POST"));

                links.Add(new LinkDto(Url.Link("UpdateUser",
                    new { id }), "update_user", "PUT"));

                links.Add(new LinkDto(Url.Link("PartiallyUpdateUser",
                    new { id }), "partially_update_user", "PATCH"));

                links.Add(new LinkDto(Url.Link("DeleteUser",
                    new { id }), "delete_user", "DELETE"));
            }

            return links;
        }

        private IEnumerable<LinkDto> CreateUsersLinks
            (UsersResourceParameters resourceParameters,
            bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>
            {
                new LinkDto(CreateUsersResourceUri(resourceParameters,
                ResourceUriType.Current), "self", "GET")
            };

            if (hasNext)
            {
                links.Add(new LinkDto(CreateUsersResourceUri(resourceParameters,
                    ResourceUriType.NextPage), "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateUsersResourceUri(resourceParameters,
                    ResourceUriType.PreviousPage), "previousPage", "GET"));
            }

            return links;
        }
    }
}