using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CEA_API.Custom;
using CEA_API.Models;
using CEA_API.Models.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace CEA_API.Controllers
{   
    //Establece la ruta api/Acceso
    [Route("api/[controller]")]
    //Establece que no se deberá ser autenticado para usar este controlador
    [AllowAnonymous]
    [ApiController]
    public class AccesoController : ControllerBase
    {
        //Instancia un objeto para la base de datos
        private readonly J54hfncyh4CeaContext _DB_CEA_Context;
        //Instancia un objeto para la creación de tokens
        private readonly Utilidades _utilidades;

        //Establece el constructor del controlador.
        public AccesoController(J54hfncyh4CeaContext DB_CEA_Context, Utilidades utilidades)
        {
            _DB_CEA_Context = DB_CEA_Context;
            _utilidades = utilidades;
        }

        //Controlador para el login de usuario. 
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(LoginDTO objeto)
        {
            try
            {
            //Busca el usuario en la base de datos.
            var usuarioEncontrado = await _DB_CEA_Context.CeaUsers
                                        .Where(u =>
                                            //Evalúa que el valor de ingreso sea el usuario o el correo
                                           (u.NameUser == objeto.usuario || u.EmailUser == objeto.usuario) &&
                                           //Evalúa que la contraseña sea la correcta.
                                           u.PassUser == _utilidades.encriptarSHA256(objeto.clave) &&
                                           //Evalúa que el usuario esté activo
                                           u.IsActive == true
                                           ).FirstOrDefaultAsync();

            //Si no encuentra el usuario o alguna credencial es incorrecta, retorna false.
            if (usuarioEncontrado == null)
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = false, token = "" });
            else
                //Si encuentra el usuario y las credenciales son correctas, retorna true, id de usuario y el id del rol de usuario.
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, token = _utilidades.generarJWT(usuarioEncontrado), usuarioEncontrado.IdUser, usuarioEncontrado.IdUsersRolUser});

            } catch(Exception ex)
            {
                //En caso de que haya un error en la búsqueda y validación del usuario, retorna falso y la notificación del error.
                return StatusCode(StatusCodes.Status500InternalServerError, new { isSuccess = false, message = $"Ocurrió un error al buscar le usuario: {ex.Message}" });
            }
        }

        //Controlador para listar los permisos asignados a los usuarios
        [HttpGet]
        [Route("ListUserPermissions/{id:int}")]
        public async Task<IActionResult> ListUserPermissions(int id)
        {
            try
            {
                //Busca en la tabla PermissionGranted
                var system = await _DB_CEA_Context.CeaPermissionsGranteds
                                        //Establece la búsqueda por id de usuario
                                        .Where(u => u.IdPermissionGrantedUser == id)
                                        //Incluye la tabla permisos 
                                        .Include(pg => pg.IdPermissionGrantedPermissionNavigation) 
                                        //Incluye la tabla sistemas 
                                            .ThenInclude(p => p.IdSystemPermissionNavigation)  
                                        .Select(u => new SystemsDTO
                                        {
                                            //Si encuentra permisos asignados al usuario con el id entregado, retorna id del sistema asignado, nombre del sistema y link.
                                            IdSystem = u.IdPermissionGrantedPermissionNavigation.IdSystemPermissionNavigation.IdSystem,
                                            NameSystem = u.IdPermissionGrantedPermissionNavigation.IdSystemPermissionNavigation.NameSystem,
                                            LinkSystems = u.IdPermissionGrantedPermissionNavigation.IdSystemPermissionNavigation.LinkSystems
                                        })
                                        .ToListAsync();
                //En caso de no encontrar sistemas asociados al usuario retorna falso y el mensaje de notificación 
                if (system == null || !system.Any())
                {
                    return Ok(new { isSuccess = false, message = "No se encontraron sistemas" });
                }
                //En caso de encontrar sistemas asociados al usuario, retorna true y los sistemas asocuados al usuario solicitado.
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, system });
            }
            catch (Exception ex)
            {
                //En caso de algún error en la búsqueda de los sistemas asociados al usuario, retorna falso y el mensaje de error generado. 
                return StatusCode(StatusCodes.Status500InternalServerError, new { isSuccess = false, message = $"Ocurrió un error al obtener la lista de sistemas: {ex.Message}" });
            }
        }
        [HttpPost]
        [Route("RestorePassword")]
        //Controlador para cambiar la contraseña
        public async Task<IActionResult> RestorePassword([FromBody] EmailsDTO value)
        {
            //Realiza la búsqueda del usuario por correo o usuario.
            var usuarioEncontrado = await _DB_CEA_Context.CeaUsers
                                        .Where(u => u.NameUser == value.Value || u.EmailUser == value.Value).FirstOrDefaultAsync();

            //En caso de no encontrar el usuario retorna false y no pasa token..
            if (usuarioEncontrado == null)
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = false, token = "" });
            else
                //En caso de encontrar le usuario retorna true, el token generado, el id del usuario y el id del rol de usuario
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, token = _utilidades.generarJWT(usuarioEncontrado), usuarioEncontrado.IdUser, usuarioEncontrado.IdUsersRolUser });
        }
    }
}
