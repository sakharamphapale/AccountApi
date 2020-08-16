using AccountApi.DomainModels;
using System.Threading.Tasks;

namespace AccountApi.Services
{
    public interface IProfileService
    {
        Task<Authentication> Authenticate(string username, string password);

        Task<int> Create(Profile profile);

        Task<Profile> GetByUsername(string username);

        Task Update(Profile profile);

        Task<Profile> ResetPassword(string userName, string oldPassword, string newPassword);
    }
}
