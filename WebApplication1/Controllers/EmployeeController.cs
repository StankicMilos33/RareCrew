using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly EmployeeService employeeServices;

        public EmployeeController(EmployeeService employeeServices)
        {
            this.employeeServices = employeeServices;
        }

        [HttpPost("createHtml")]
        public async Task<IActionResult> CreateHtml()
        {
            await this.employeeServices.CreateHtml();
            return Ok();
        }

        [HttpPost("CreateImage")]
        public async Task<IActionResult> CreateImage()
        {
            await this.employeeServices.CreatePieChart();
            return Ok();
        }
    }
}