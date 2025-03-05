using backend.myStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace backend.myStore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CuentasController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        public CuentasController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Welcome()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Ok("El Usuario no está autenticado");
            }

            return Ok("Usted está autenticado");
        }

        [Authorize]
        [HttpGet("Perfil")]
        public async Task<IActionResult> Perfil()
        {
            var usuarioActual = await _userManager.GetUserAsync(User);
            if(usuarioActual == null)
            {
                return BadRequest();
            }

            var perfilUsuario = new UserProfile
            {
                Id = usuarioActual.Id,
                Name = usuarioActual.UserName ?? "",
                Email = usuarioActual.Email ?? "",
                PhoneNumber = usuarioActual.PhoneNumber ?? ""
            };

            return Ok(perfilUsuario);
        }



    }
}
