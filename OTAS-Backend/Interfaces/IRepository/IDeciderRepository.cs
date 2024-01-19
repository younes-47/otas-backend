using OTAS.Models;

namespace OTAS.Interfaces.IRepository
{
    public interface IDeciderRepository
    {
        Task<int> GetManagerUserIdByUserIdAsync(int  userId);
        Task<int> GetDeciderUserIdByDeciderLevel(string level);
        Task<string?> GetDeciderLevelByUserId(int deciderUserId);
        Task<Decider?> FindDeciderByUserIdAsync(int Id);
    }
}
