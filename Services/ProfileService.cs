using AccountApi.DomainModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AccountApi.Services
{
    public class ProfileService : IProfileService
    {
        private readonly Data.AccountsDbContext _dbConext;
        private readonly AutoMapper.IMapper _mapper;
        private readonly IConfiguration _configuration;

        public ProfileService(Data.AccountsDbContext dbConext, AutoMapper.IMapper mapper, IConfiguration configuration)
        {
            _dbConext = dbConext;
            _mapper = mapper;
            _configuration = configuration;
        }

        public async Task<int> Create(Profile profile)
        {
            // Generate password salt and hash
            ComputePasswordKeys(profile.Password, out byte[] passwordSalt, out byte[] passwordHash);

            // Store profile in database
            var profileData = new Data.Profile
            {
                UserName = profile.Username,
                FirstName = profile.FirstName,
                LastName = profile.LastName,
                EmailAddress = profile.EmailAddress,
                PhoneNumber = profile.PhoneNumber,
                DateOfBirth = profile.DateOfBirth,
                PasswordSalt = passwordSalt,
                PasswordHash = passwordHash,
                CreatedTs = DateTime.UtcNow
            };

            _dbConext.Profile.Add(profileData);

            await _dbConext.SaveChangesAsync();

            return profileData.Id;

            // perform additional setup

            // Send confirmation email to user
        }

        public async Task<Profile> GetByUsername(string username)
        {
            var existingProfileData = await _dbConext.Profile.SingleOrDefaultAsync(x => x.UserName.Equals(username));

            if (existingProfileData == null)
                return null;

            var profile = new Profile
            {
                Username = existingProfileData.UserName,
                FirstName = existingProfileData.FirstName,
                LastName = existingProfileData.LastName,
                EmailAddress = existingProfileData.EmailAddress,
                PhoneNumber = existingProfileData.PhoneNumber,
                DateOfBirth = existingProfileData.DateOfBirth,
                PasswordHash = existingProfileData.PasswordHash,
                PasswordSalt = existingProfileData.PasswordSalt,
                CreatedAt = existingProfileData.CreatedTs,
                UpdatedAt = existingProfileData.UpdatedTs
            };

            return profile;
        }

        public async Task Update(Profile profile)
        {
            var existingProfileData = await _dbConext.Profile.SingleOrDefaultAsync(x => x.UserName.Equals(profile.Username));

            if (existingProfileData == null)
                return;

            if (!existingProfileData.UserName.Equals(profile.Username))
                existingProfileData.UserName = profile.Username;
            
            existingProfileData.FirstName = profile.FirstName;
            existingProfileData.LastName = profile.LastName;
            existingProfileData.EmailAddress = profile.EmailAddress;
            existingProfileData.PhoneNumber = profile.PhoneNumber;
            existingProfileData.DateOfBirth = profile.DateOfBirth;

            if (!string.IsNullOrWhiteSpace(profile.Password))
            {
                // Generate password salt and hash
                ComputePasswordKeys(profile.Password, out byte[] passwordSalt, out byte[] passwordHash);

                existingProfileData.PasswordSalt = passwordSalt;
                existingProfileData.PasswordHash = passwordHash;
            }

            existingProfileData.UpdatedTs = DateTime.UtcNow;

            await _dbConext.SaveChangesAsync();

            // send confirmation email

        }

        public async Task<Authentication> Authenticate(string username, string password)
        {
            // Get profile and stored password hash from database
            var profile = await GetByUsername(username);

            // Hash input passowrd
            var inputPasswordHash = GetPasswordHash(profile.PasswordSalt, password);

            // Match input password hash with stored password hash
            if (inputPasswordHash.Length != profile.PasswordHash.Length)
            {
                return null;
            }

            for (var index = 0; index < inputPasswordHash.Length; index++)
            {
                if (inputPasswordHash[index] != profile.PasswordHash[index])
                {
                    return null;
                }
            }

            // Generate token
            var securityKey = System.Text.Encoding.ASCII.GetBytes(_configuration.GetValue<string>("JwtTokenSecretKey"));
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, profile.Username)
                }),
                Expires = DateTime.UtcNow.AddMinutes(20),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(securityKey), SecurityAlgorithms.HmacSha256)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            var authentication = _mapper.Map<Authentication>(profile);
            authentication.Token = tokenString;

            return authentication;
        }

        public async Task<Profile> ResetPassword(string userName, string oldPassword, string newPassword)
        {
            var existingProfileData = await _dbConext.Profile.SingleOrDefaultAsync(x => x.UserName.Equals(userName));

            if (existingProfileData == null)
                return null;

            // Hash input passowrd
            var inputPasswordHash = GetPasswordHash(existingProfileData.PasswordSalt, oldPassword);

            // Match input password hash with stored password hash
            if (inputPasswordHash.Length != existingProfileData.PasswordHash.Length)
            {
                return null;
            }

            for (var index = 0; index < inputPasswordHash.Length; index++)
            {
                if (inputPasswordHash[index] != existingProfileData.PasswordHash[index])
                {
                    return null;
                }
            }

            // Generate password salt and hash
            ComputePasswordKeys(newPassword, out byte[] passwordSalt, out byte[] passwordHash);

            existingProfileData.PasswordSalt = passwordSalt;
            existingProfileData.PasswordHash = passwordHash;

            existingProfileData.UpdatedTs = DateTime.UtcNow;

            await _dbConext.SaveChangesAsync();

            var profile = new Profile
            {
                Username = existingProfileData.UserName,
                FirstName = existingProfileData.FirstName,
                LastName = existingProfileData.LastName,
                EmailAddress = existingProfileData.EmailAddress,
                PhoneNumber = existingProfileData.PhoneNumber,
                DateOfBirth = existingProfileData.DateOfBirth,
                CreatedAt = existingProfileData.CreatedTs,
                UpdatedAt = existingProfileData.UpdatedTs
            };

            return profile;
            // send confirmation email
        }

        private void ComputePasswordKeys(string password, out byte[] passwordSalt, out byte[] passwordHash)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private byte[] GetPasswordHash(byte[] passwordSalt, string password)
        {
            byte[] passwordHash;

            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                hmac.Key = passwordSalt;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }

            return passwordHash;
        }
    }
}
