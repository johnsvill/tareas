using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using Tareas.Data;
using Tareas.Models;
using Tareas.Servicios;

namespace Tareas.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDbContext _dbContext;

        public UsuariosController(UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager, ApplicationDbContext dbContext)
        {
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._dbContext = dbContext;
        }

        [AllowAnonymous]
        public async Task<ActionResult> Registro()
        {
            return await Task.Run(() => View());
        }

        [HttpPost, ActionName("Registro")]
        [AllowAnonymous]
        public async Task<ActionResult> Registro(RegistroViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return await Task.Run(() => View(modelo));
            }

            var usuario = new IdentityUser()
            {
                Email = modelo.Email,
                UserName = modelo.Email
            };

            var resultado = await this._userManager.CreateAsync(usuario, password: modelo.Password);

            if (resultado.Succeeded)
            {
                await this._signInManager.SignInAsync(usuario, isPersistent: true);

                return RedirectToAction("Index", "Home");
            }
            else
            {
                foreach(var error in resultado.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return await Task.Run(() => View(modelo));
            }            
        }

        [AllowAnonymous]
        public async Task<ActionResult> Login(string mensaje = null)
        {
            if(mensaje is not null)
            {
                ViewData["Mensaje"] = mensaje;
            }

            return await Task.Run(() => View());    
        }

        [HttpPost, ActionName("Login")]
        [AllowAnonymous]
        public async Task<ActionResult> Login(LoginViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return await Task.Run(() => View(modelo));
            }

            var resultado = await this._signInManager.PasswordSignInAsync(modelo.Email,
                modelo.Password, modelo.Recuerdame, lockoutOnFailure: false);

            if (resultado.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Nombre de usuario o password incorrecto.");

                return await Task.Run(() => View(modelo));
            }            
        }

        [HttpPost, ActionName("Logout")]        
        public async Task<ActionResult> Logout()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        [HttpGet]
        public ChallengeResult LoginExterno(string proveedor, string urlRetorno = null)
        {
            var urlRedireccion = Url.Action("RegistrarUsuarioExterno", values: new { urlRetorno });
            var propiedades = this._signInManager.ConfigureExternalAuthenticationProperties(proveedor, urlRedireccion);

            return new ChallengeResult(proveedor, propiedades);
        }

        [AllowAnonymous]
        public async Task<ActionResult> RegistrarUsuarioExterno(string urlRetorno = null,
            string remoteError = null)
        {
            urlRetorno = urlRetorno ?? Url.Content("~/");

            var mensaje = "";

            if(remoteError is not null)
            {
                mensaje = $"Error del proveedor extorno: { remoteError }";
                return RedirectToAction("Login", routeValues: new { mensaje });
            }

            var info = await this._signInManager.GetExternalLoginInfoAsync();

            if(info is null)
            {
                mensaje = "Error cargando la data de login externo";
                return RedirectToAction("Login", routeValues: new { mensaje });
            }

            var resultadoLoginExterno = await this._signInManager.ExternalLoginSignInAsync(info.LoginProvider,
                info.ProviderKey, isPersistent: true, bypassTwoFactor: true);

            //Ya la cuenta existe
            if (resultadoLoginExterno.Succeeded)
            {
                return LocalRedirect(urlRetorno);
            }

            string email = "";

            if(info.Principal.HasClaim(x => x.Type == ClaimTypes.Email))
            {
                email = info.Principal.FindFirstValue(ClaimTypes.Email);
            }
            else
            {
                mensaje = "Error leyendo el email del usuario del proveedor";
                return RedirectToAction("Login", routeValues: new { mensaje });
            }

            var usuario = new IdentityUser { Email = email, UserName = email };

            var resultadoCrearUsuario = await this._userManager.CreateAsync(usuario);

            if (!resultadoCrearUsuario.Succeeded)
            {
                mensaje = resultadoCrearUsuario.Errors.First().Description;
                return RedirectToAction("Login", routeValues: new { mensaje });
            }

            var resultadoAgregarLogin = await this._userManager.AddLoginAsync(usuario, info);

            if (resultadoAgregarLogin.Succeeded)
            {
                await this._signInManager.SignInAsync(usuario, isPersistent: true, info.LoginProvider);
                return LocalRedirect(urlRetorno);
            }

            mensaje = "Ha ocurrido un error agregando el login";
            return RedirectToAction("Login", routeValues: new { mensaje });
        }

        [HttpGet]
        [Authorize(Roles = Constantes.RolAdmin)]
        public async Task<ActionResult> Listado(string mensaje = null)
        {
            var usuarios = await this._dbContext.Users.Select(x => new UsuarioViewModel
            {
                Email = x.Email
            }).ToListAsync();

            var modelo = new UsuariosListadoViewModel();
            modelo.Usuarios = usuarios;
            modelo.Mensaje = mensaje;

            return await Task.Run(() => View(modelo));
        }

        [Authorize(Roles = Constantes.RolAdmin)]
        public async Task<ActionResult> HacerAdmin(string email)
        {
            var usuario = await this._dbContext.Users.Where(x =>
                        x.Email == email).DefaultIfEmpty().FirstOrDefaultAsync();

            if(usuario is null)
            {
                return NotFound();
            }

            await this._userManager.AddToRoleAsync(usuario, Constantes.RolAdmin);

            return RedirectToAction("Listado",
                routeValues: new { mensaje = $"Rol asignado correctamente a {email}" });
        }

        [Authorize(Roles = Constantes.RolAdmin)]
        public async Task<ActionResult> RemoverAdmin(string email)
        {
            var usuario = await this._dbContext.Users.Where(x =>
                        x.Email == email).DefaultIfEmpty().FirstOrDefaultAsync();   

            if (usuario is null)
            {
                return NotFound();
            }

            await this._userManager.RemoveFromRoleAsync(usuario, Constantes.RolAdmin);

            return RedirectToAction("Listado",
                routeValues: new { mensaje = $"Rol removido correctamente a {email}" });
        }
    }
}
