using Hospital.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Hospital.Core.DTOs;
using Microsoft.EntityFrameworkCore;
using Hospital.API.Data;

namespace Hospital.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public AuthController(UserManager<ApplicationUser> userManager, IConfiguration configuration, ApplicationDbContext context)
        {
            _userManager = userManager;
            _configuration = configuration;
            _context = context;
        }
        [AllowAnonymous]
        [HttpPost("Login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Login([FromBody] LoginDto model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if(user != null && user.IsActive && !user.IsDeleted && await _userManager.CheckPasswordAsync(user,model.Password))
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var AuthClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("FullName", user.FullName),
                };
                foreach (var UserRole in userRoles)
                {
                    AuthClaims.Add(new Claim(ClaimTypes.Role, UserRole));
                }
                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    expires: DateTime.Now.AddDays(Convert.ToDouble(_configuration["Jwt:DurationInDays"])),
                    claims: AuthClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );
                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo,
                    username = user.UserName
                });
            }
            return Unauthorized(new { message = "اسم المستخدم أو كلمة المرور غير صحيحة" });
        }
        [HttpPost("Register")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] RegisterDTO model)
        {
            if (await _userManager.FindByNameAsync(model.UserName) != null)
                return BadRequest(new { message = "اسم المستخدم موجود مسبقاً" });

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                FullName = model.FullName,
                EmployeeId = model.EmployeeId 
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User"); 
                return Ok(new { message = "تم إنشاء الحساب بنجاح" });
            }
            return BadRequest(new { message = "فشل إنشاء المستخدم", errors = result.Errors.Select(e => e.Description) });
        }
        [HttpPost("ChangePassword")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized(new { message = "المستخدم غير موجود أو الجلسة انتهت" });

            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (result.Succeeded) return Ok(new { message = "تم تغيير كلمة المرور بنجاح" });

            return BadRequest(new { message = "فشل التغيير", errors = result.Errors.Select(e => e.Description) });
        }
        [HttpPost("AdminResetPassword")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AdminResetPassword([FromBody] AdminResetDto model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null) return NotFound(new { message = "المستخدم غير موجود" });

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

            if (result.Succeeded) return Ok(new { message = "تمت إعادة تعيين كلمة المرور بنجاح" });
            return BadRequest(new { message = "فشل التغيير", errors = result.Errors.Select(e => e.Description) });
        }
        [HttpGet("Users")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetUsers([FromQuery] bool? IsDeleted, [FromQuery] bool? IsActive)
        {
            IQueryable<ApplicationUser> query =  _context.Users.IgnoreQueryFilters().AsNoTracking();
            if (IsDeleted.HasValue)
            {
                query = query.Where(e => e.IsDeleted == IsDeleted.Value);
            }
            if (IsActive.HasValue)
            {
                query = query.Where(e => e.IsActive == IsActive.Value);
            }
            var usersWithRoles = await query
                .Select(user => new UserViewDTO
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    FullName = user.FullName,
                    EmployeeId = user.EmployeeId,
                    Role = _context.UserRoles
                        .Where(ur => ur.UserId == user.Id)
                        .Join(_context.Roles,
                              ur => ur.RoleId,
                              role => role.Id,
                              (ur, role) => role.Name)
                        .FirstOrDefault(),
                    IsActive = user.IsActive,
                    IsDeleted = user.IsDeleted,
                }).ToListAsync();

            return Ok(usersWithRoles);
        }
        [HttpPut("UpdateUser")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateUser([FromBody] UserViewDTO model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);

            if (user == null)
                return NotFound(new { message = "المستخدم غير موجود" });
            user.FullName = model.FullName;
            user.IsActive = model.IsActive;
            user.IsDeleted = model.IsDeleted;
            user.EmployeeId = model.EmployeeId;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(model.Role))
                {
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    if (!currentRoles.Contains(model.Role))
                    {
                        await _userManager.RemoveFromRolesAsync(user, currentRoles);
                        await _userManager.AddToRoleAsync(user, model.Role);
                    }
                }

                return Ok(new { message = "تم تحديث بيانات المستخدم بنجاح" });
            }

            return BadRequest(new { message = "فشل تحديث البيانات", errors = result.Errors.Select(e => e.Description) });
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound(new { message = "المستخدم غير موجود" });

            // تنفيذ الحذف المنطقي (Soft Delete)
            user.IsDeleted = true;
            user.IsActive = false; // تعطيل الحساب تلقائياً عند الحذف

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return Ok(new { message = "تم حذف المستخدم (نقل للأرشيف) بنجاح" });
            }

            return BadRequest(new { message = "فشل في عملية الحذف", errors = result.Errors.Select(e => e.Description) });
        }
    }
}
