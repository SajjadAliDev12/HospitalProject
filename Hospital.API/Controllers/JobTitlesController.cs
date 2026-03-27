using Hospital.API.Data;
using Hospital.Core.DTOs;
using Hospital.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hospital.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobTitlesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public JobTitlesController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<JobTitleDTO>>> GetJobTitles()
        {
            return Ok(await _context.JobTitles
                .Select(j => new JobTitleVeiwDTO { Id = j.Id, Title = j.Title })
                .ToListAsync());
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Create(JobTitleDTO dto)
        {
            var jobTitle = new JobTitle { Title = dto.Title };
            _context.JobTitles.Add(jobTitle);
            await _context.SaveChangesAsync();
            return Ok(new { message = "تمت إضافة العنوان الوظيفي بنجاح" });
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Update(int id, JobTitleVeiwDTO dto)
        {
            var job = await _context.JobTitles.FindAsync(id);
            if (job == null) return NotFound();
            job.Title = dto.Title;
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Delete(int id)
        {
            var job = await _context.JobTitles.FindAsync(id);
            if (job == null) return NotFound();
            _context.JobTitles.Remove(job);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }

}
