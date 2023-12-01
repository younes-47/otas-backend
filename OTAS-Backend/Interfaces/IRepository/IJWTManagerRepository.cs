
using OTAS.Models;

namespace OTAS.Interfaces.IRepository
{
    public interface IJWTManagerRepository
    {
        Tokens CreateToken(string username, int role);
    }
}
