using Hospital.API.Data;
using Hospital.Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hospital.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AuditLogsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public AuditLogsController(ApplicationDbContext context)
        {  _context = context; }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<AuditLogDTO>>> GetLogs()
        { 
            return Ok(await _context.AuditLogs.OrderByDescending(a => a.Date).Select(a => new AuditLogDTO
            {
                 Id = a.Id,
                 Date = a.Date,
                 UserId = a.UserId,
                 ActionType = a.Type.ToString()
            }).ToListAsync());
        }
    }
}
