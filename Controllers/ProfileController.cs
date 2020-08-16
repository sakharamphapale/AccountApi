using AccountApi.Models;
using AccountApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AccountApi.Controllers
{
    [Route("api/profile")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;
        private readonly AutoMapper.IMapper _mapper;

        public ProfileController(IProfileService profileService, AutoMapper.IMapper mapper)
        {
            _profileService = profileService;
            _mapper = mapper;
        }

        [Route("ping")]
        [HttpGet]
        public IActionResult Ping()
        {
            return Ok("I AM ONLINE !!!");
        }

        [Route("register")]
        [HttpPost]
        public async Task<IActionResult> Register(RegistrationRequest request)
        {
            try
            {
                var profile = _mapper.Map<DomainModels.Profile>(request);

                var profileId = await _profileService.Create(profile);
                return Ok(profileId);
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        [Route("authenticate")]
        [HttpPost]
        public async Task<ActionResult> Authenticate(AuthenticateRequest request)
        {
            var authenticate = await _profileService.Authenticate(request.Username, request.Password);

            if (authenticate == null)
            {
                return BadRequest("Username and Password combination is incorrect");
            }

            var authResponse = new AuthenticateResponse
            {
                Username = authenticate.Username,
                FirstName = authenticate.FirstName,
                LastName = authenticate.LastName,
                Token = authenticate.Token
            };

            return Ok(authResponse);
        }

        [Route("passwordreset")]
        [HttpPost]
        public async Task<IActionResult> PasswordReset(PasswordResetRequest request)
        {
            try
            {
                var profile = await _profileService.ResetPassword(request.Username, request.OldPassword, request.NewPassword);

                if (profile == null)
                {
                    return BadRequest("Username and Old Password combination is incorrect");
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        [Authorize]
        [Route("{username}")]
        [HttpGet]
        public async Task<ActionResult> GetUser(string username)
        {
            var profileData = await _profileService.GetByUsername(username);

            if (profileData == null)
            {
                return NotFound();
            }

            var profileModel = new ProfileModel
            {
                Username = profileData.Username,
                FirstName = profileData.FirstName,
                LastName = profileData.LastName,
                DateOfBirth = profileData.DateOfBirth,
                PhoneNumber = profileData.PhoneNumber,
                EmailAddress = profileData.EmailAddress,
                CreatedAt = profileData.CreatedAt,
                UpdatedAt = profileData.UpdatedAt
            };

            return Ok(profileModel);
        }
    }
}
