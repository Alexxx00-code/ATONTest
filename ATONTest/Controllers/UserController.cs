using Application.Interfaces;
using Application.Models;
using ATONTest.DTOModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ATONTest.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private IUserService _userService;
        private readonly IHttpContextAccessor _context;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("login")]
        public ActionResult<string> Login(AuthDTO userDTO)
        {
            var user = _userService.CheakPassword(userDTO.Login, userDTO.Password);
            if (user != null)
            {
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Login),
                    new Claim(ClaimTypes.NameIdentifier, user.Guid.ToString())
                };

                if (user.Admin)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, "Admin"));
                }

                return Ok(CreateToken(authClaims));
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPost()]
        [Authorize(Roles = "Admin")]
        public ActionResult<UserModel> CreateUser(CreateUserDTO createUser)
        {
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;

            if(userName == null)
            {
                return Unauthorized();
            }

            try
            {
                UserModel userResp = _userService.Create(new CreateUserModel
                {
                    Name = createUser.Name,
                    Birthday = createUser.Birthday,
                    Gender = createUser.Gender,
                    Login = createUser.Login,
                    Admin = createUser.Admin,
                    Password = createUser.Password,
                    CreatedBy = userName,
                });

                return Ok(userResp);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{guid}")]
        [Authorize(Roles = "Admin")]
        public ActionResult<UserModel> UpdateUserByGuid(UpdateUserDTO updateUser, Guid guid)
        {
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;

            if (userName == null)
            {
                return Unauthorized();
            }

            try
            {
                UserModel userResp = _userService.UpdateUser(new UpdateUserModel
                {
                    Guid = guid,
                    Name = updateUser.Name,
                    Login = updateUser.Login,
                    Birthday = updateUser.Birthday,
                    Gender = updateUser.Gender,
                    ModifiedBy = userName
                });

                return Ok(userResp);
            }
            catch(InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut()]
        [Authorize]
        public ActionResult<UserModel> UpdateUser(UpdateUserDTO updateUser)
        {
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            var userGuidStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userName == null)
            {
                return Unauthorized();
            }

            Guid userGuid = Guid.Parse(userGuidStr);

            try
            {
                UserModel userResp = _userService.UpdateUser(new UpdateUserModel
                {
                    Guid = userGuid,
                    Name = updateUser.Name,
                    Login = updateUser.Login,
                    Birthday = updateUser.Birthday,
                    Gender = updateUser.Gender,
                    ModifiedBy = userName
                });

                return Ok(userResp);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("updatePassword/{guid}")]
        [Authorize(Roles = "Admin")]
        public ActionResult<UserModel> UpdatePasswordByGuid([FromBody] string newPassword, Guid guid)
        {
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;

            if (userName == null)
            {
                return Unauthorized();
            }

            UserModel userResp = _userService.UpdatePassword(new UpdatePassword
            {
                Guid = guid,
                Password = newPassword,
                ModifiedBy = userName
            });

            return Ok(userResp);
        }

        [HttpPut("updatePassword")]
        [Authorize]
        public ActionResult<UserModel> UpdatePassword([FromBody] string newPassword)
        {
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            var userGuidStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userName == null)
            {
                return Unauthorized();
            }

            Guid userGuid = Guid.Parse(userGuidStr);


            UserModel userResp = _userService.UpdatePassword(new UpdatePassword
            {
                Guid = userGuid,
                Password = newPassword,
                ModifiedBy = userName
            });

            return Ok(userResp);
        }

        [HttpGet("list")]
        [Authorize(Roles = "Admin")]
        public ActionResult<List<UserModel>> GetList([FromQuery] uint minAge = 0, [FromQuery] bool revoked = false)
        {
            return Ok(_userService.GetList(minAge, revoked));
        }

        [HttpGet("one/{login}")]
        [Authorize(Roles = "Admin")]
        public ActionResult<UserModel> GetUser(string login)
        {
            var user = _userService.GetByLogin(login);
            if (user == null)
            {
                return BadRequest("User not found");
            }

            return Ok(user);
        }

        [HttpGet()]
        [Authorize]
        public ActionResult<UserModel> GetUser()
        {
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (userName == null)
            {
                return Unauthorized();
            }

            return Ok(_userService.GetByLogin(userName));
        }

        [HttpDelete()]
        [Authorize(Roles = "Admin")]
        public ActionResult<UserModel> Delete(DeleteDTO dto)
        {
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (userName == null)
            {
                return Unauthorized();
            }

            try
            {
                _userService.RemoveUser(dto.Login, dto.Hard, userName);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("restore")]
        [Authorize(Roles = "Admin")]
        public ActionResult<UserModel> Restore([FromBody] string login)
        {
            try
            {
                _userService.RestoreUser(login);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private string CreateToken(List<Claim> authClaims)
        {
            var jwt = new JwtSecurityToken(
                        issuer: AuthOptions.ISSUER,
                        audience: AuthOptions.AUDIENCE,
                        claims: authClaims,
                        expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(30)), // время действия 2 минуты
                        signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            
            return new JwtSecurityTokenHandler().WriteToken(jwt); ;
        }
    }
}
