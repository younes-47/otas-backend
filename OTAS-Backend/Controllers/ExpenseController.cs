using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OTAS.Interfaces.IRepository;

namespace OTAS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpenseController : ControllerBase
    {
        private readonly IExpenseRepository _expenseRepository;
        public ExpenseController(IExpenseRepository expenseRepository)
        {
            _expenseRepository = expenseRepository;
        }




    }
}
