using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CEA_API.Custom;
using CEA_API.Models;
using CEA_API.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace CEA_API.Controllers
{

    //Establece una ruta para el controlador /api/User
    [Route("api/[controller]")]
    //Establece que para dar uso a los métodos del controlador, se debe de estar autenticado.
    [Authorize]
    [ApiController]

    public class UserController : ControllerBase
    {
        //Instancia un objeto para la base de datos
        private readonly J54hfncyh4CeaContext _DB_CEA_Context;
        //Instancia un objeto para utilidades
        private readonly Utilidades _utilidades;
        //Establece el constructor del controlador.
        public UserController(J54hfncyh4CeaContext DB_CEA_Context, Utilidades utilidades)
        {
            _DB_CEA_Context = DB_CEA_Context;
            _utilidades = utilidades;
        }

        //Establece un método Post
        [HttpPost]
        //Establece la ruta para el método /api/User/register
        [Route("register")]
        //Recibe del body un objeto de tipo UsuarioDTO
        public async Task<IActionResult> register([FromBody] UsuarioDTO objeto)
        {
            //Instancia un nuevo objeto de tipo CeaUser
            var modeloUsuario = new CeaUser
            {
                //Entrega al objeto de tipo CeaUser los datos entregados por el usuario en el body
                NameUser = objeto.NameUser,
                EmailUser = objeto.EmailUser,
                PassUser = _utilidades.encriptarSHA256(objeto.PassUser),
                IdUsersRolUser = objeto.IdUsersRolUser,
                IsActive = objeto.IsActive,
                LastLogin = null,
                EmailState = false
            };
            try
            {
                //Agrega el usuario a la base de datos.
                await _DB_CEA_Context.CeaUsers.AddAsync(modeloUsuario);
                //Guarda cambios en la base de datos.
                await _DB_CEA_Context.SaveChangesAsync();

                //En caso de que retorne el id del usuario, significa que si se grabó el usuario.
                //Retorna true, estado 201 y el id del usuario
                if (modeloUsuario.IdUser != 0)
                {
                    return StatusCode(StatusCodes.Status201Created, new { isSuccess = true, userId = modeloUsuario.IdUser, message = "Usuario registrado exitosamente." });
                }
                else
                {
                    //En caso de que no retorne el id del usuario, la api retorna false y estado 500
                    return StatusCode(StatusCodes.Status500InternalServerError, new { isSuccess = false, message = "El registro del usuario falló." });
                }
            }
            catch (Exception ex)
            {
                //En caso de que se presente algún error dentro del try, retorna false, estado 500 y el cuerpo del error.
                return StatusCode(StatusCodes.Status500InternalServerError, new { isSuccess = false, message = $"Ocurrió un error durante el registro: {ex.Message}" });
            }
        }

        //Establece un métoo Get
        [HttpGet]
        //Establece la ruta para el metodo /api/User/ListUser
        [Route("ListUsers")]
        public async Task<IActionResult> ListUsers()
        {
            try
            {
                //Realiza la búsqueda unicamente del os usuarios activos
                var users = await _DB_CEA_Context.CeaUsers
                                .Where(u => u.IsActive == true)
                                .Select(u => new UsuarioDTO 
                                {
                                    //Realiza la selección de los atributos que se quieren listar de los usuarios
                                    IdUser = u.IdUser,
                                    NameUser = u.NameUser,
                                    EmailUser = u.EmailUser,
                                    IdUsersRolUser = u.IdUsersRolUser,
                                    IsActive = u.IsActive,
                                    LastLogin = u.LastLogin
                                    
                                })
                                .ToListAsync();

                //En caso de que no encuentre usuarios, retorna false y estado 404
                if (users == null || !users.Any())
                {
                    return NotFound(new { isSuccess = false, message = "No se encontraron usuarios." });
                }
                //En caso de que si encuentre usuarios, retorna true, estado 200 y la lista de usuarios.
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, users });
            }
            catch (Exception ex)
            {
                //En caso de que se presente un error dentro del try, retorna false, estado 500 y el cuerpo del error
                return StatusCode(StatusCodes.Status500InternalServerError, new { isSuccess = false, message = $"Ocurrió un error al obtener la lista de usuarios: {ex.Message}" });
            }
        }

        //Establece un método Get
        [HttpGet]
        //Establece la ruta para el método /api/User/get/id
        [Route("get/{id:int}")] 
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                //Busca en la base de datos el usuario con el id suministrado
                var user = await _DB_CEA_Context.CeaUsers.Where(u => u.IdUser == id)
                                .Select(u => new EmailStateDTO
                                {
                                    emailstate = u.EmailState
                                })
                                .FirstOrDefaultAsync();
                //En caso de que no encuentre el usuario retorna false y estado 404
                if (user == null)
                {
                    return NotFound(new { isSuccess = false, message = $"Usuario con ID {id} no encontrado." });
                }
                //En caso de que encuentre el usuario retorna true, estado 200 y el usuario buscado.
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, user });
            }
            catch (Exception ex)
            {
                //En caso de presentarse un error dentro del try, retorna false, estado 500 y el cuerpo del error
                return StatusCode(StatusCodes.Status500InternalServerError, new { isSuccess = false, message = $"Ocurrió un error al obtener el usuario: {ex.Message}" });
            }
        }

        //Establece un metodo Put
        [HttpPut]
        //Establece una ruta para el metodo /api/User/update/id
        [Route("update/{id:int}")]
        //Recibe del body un objeto de tipo UserDTO
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserDTO objeto)
        {
            /*ModelState representa el estado del modelo de datos. En caso de ser no estar válido 
            retorna estado 400*/
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                //Realiza la búsqueda del usuario basado en el id entregado.
                var userToUpdate = await _DB_CEA_Context.CeaUsers.FirstOrDefaultAsync(u => u.IdUser == id);
                //En caso de no encontrar usuario retorna false y estado 404
                if (userToUpdate == null)
                {
                    return NotFound(new { isSuccess = false, message = $"Usuario con ID {id} no encontrado." });
                }
                //En caso de encontrar el usuario, actualiza los atributos del mismo basado en el objeto que entregó el usuario en el body
                userToUpdate.NameUser = objeto.NameUser;
                userToUpdate.EmailUser = objeto.EmailUser;
                userToUpdate.IdUsersRolUser = objeto.IdUsersRolUser;
                //Realiza la actualización del usuario en la base de datos.
                _DB_CEA_Context.CeaUsers.Update(userToUpdate);
                //Guarda los cambios en la base de datos 
                await _DB_CEA_Context.SaveChangesAsync();
                //Si la operación concluye bien, retorna true y estado 200
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, message = "Usuario actualizado exitosamente." });
            }
            catch (DbUpdateConcurrencyException)
            {
                //En caso de presentarse un error en las lineas de actualización en la base de datos, retorna false, estado 409 y el cuerpo del error
                return StatusCode(StatusCodes.Status409Conflict, new { isSuccess = false, message = "Conflicto de concurrencia. El usuario fue modificado por otro proceso." });
            }
            catch (Exception ex)
            {
                //En caso de presentarse un error dentor del try, retorna false, estado 500 y el cuerpo del error
                return StatusCode(StatusCodes.Status500InternalServerError, new { isSuccess = false, message = $"Ocurrió un error al actualizar el usuario: {ex.Message}" });
            }
        }

        //Establece un metodo Delete
        [HttpDelete]
        //Establece una ruta para el metodo /api/User/DeleteUser/id
        [Route("DeleteUser/{id:int}")] 
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                //Realiza la búsqueda del usuario basado en el id entregado
                var userToUpdate = await _DB_CEA_Context.CeaUsers.FirstOrDefaultAsync(u => u.IdUser == id);
                //En caso de no encontrar el usuario retorna false y estado 404
                if (userToUpdate == null)
                {
                    return NotFound(new { isSuccess = false, message = $"Usuario con ID {id} no encontrado." });
                }

                //En caso de que encuentre el usuario, elimina el usuario de la base de datos.
                _DB_CEA_Context.CeaUsers.Remove(userToUpdate); 
                //Guarda cambios en la base de datos
                await _DB_CEA_Context.SaveChangesAsync(); 

                //Si el proceso con la base de datos es exitoso, returna true y estado 200
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, message = "Usuario eliminado correctamente" });
            }
            catch (DbUpdateConcurrencyException)
            {
                //Si se presenta un error con la base de datos, retorna false y estado 409
                return StatusCode(StatusCodes.Status409Conflict, new { isSuccess = false, message = "Conflicto de concurrencia. El usuario fue modificado por otro proceso." });
            }
            catch (Exception ex)
            {
                //En caso de que se presente un error dentro del try, retorna false, estado 500 y el cuerpo del error
                return StatusCode(StatusCodes.Status500InternalServerError, new { isSuccess = false, message = $"Ocurrió un error al actualizar la fecha de último inicio de sesión: {ex.Message}" });
            }
        }

        //Establece un metodo Patch
        [HttpPatch]
        //Establece la ruta para acceder al metodo /api/User/update/id/lastlogin
        [Route("update/{id:int}/lastlogin")]
        //Recibe un id y la hora de ultimo acceso 
        public async Task<IActionResult> UpdateUserLastLogin(int id,[FromBody] DateTime? newLastLogin)
        {
            try
            {
                //Instancia un usuario de tipo CeaUsers, realiza la búsqueda del usuario basado en el id entregado.
                var userToUpdate = await _DB_CEA_Context.CeaUsers.FirstOrDefaultAsync(u => u.IdUser == id);
                //Si no encuentra el usuario, retorna false y estado 404
                if (userToUpdate == null)
                {
                    return NotFound(new { isSuccess = false, message = $"Usuario con ID {id} no encontrado." });
                }
                //En caso de que si encuentre el usuario, actualiza el ultimo ingreso de de la instancia CeaUsers.
                userToUpdate.LastLogin = newLastLogin;

                //Actualiza parcialmente el usuario 
                _DB_CEA_Context.CeaUsers.Update(userToUpdate);
                //Guarda cambios en la base de datos.
                await _DB_CEA_Context.SaveChangesAsync();
                //Si el proceso termina de manera exitosa, retorna truey estado 200
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, message = "Fecha de último inicio de sesión del usuario actualizada exitosamente." });
            }
            catch (DbUpdateConcurrencyException)
            {
                //Si ocurre algun error en la interacción con la base de datos, retorna false y estado 409
                return StatusCode(StatusCodes.Status409Conflict, new { isSuccess = false, message = "Conflicto de concurrencia. El usuario fue modificado por otro proceso." });
            }
            catch (Exception ex)
            {
                //Si ocurre algún error dentro del try, retorna false, estado 500 y el cuerpo del error
                return StatusCode(StatusCodes.Status500InternalServerError, new { isSuccess = false, message = $"Ocurrió un error al actualizar la fecha de último inicio de sesión: {ex.Message}" });
            }
        }

        //Establece un metodo Patch
        [HttpPatch]
        //Establece la ruta para el método /api/User/updateEmailState
        [Route("updateEmailState")]
        //Recibe del body un objeto de tipo EmailStateDTO
        public async Task<IActionResult> UpdateEmailState([FromBody] EmailStateDTO objeto)
        {
            try
            {
                //Instancia un usuario de tipo CeaUsers. Realiza la búsqueda del usuario en la base de datios basado en el usuario 
                var userToUpdate = await _DB_CEA_Context.CeaUsers.FirstOrDefaultAsync(u => u.IdUser == objeto.userId);
                //En caso de que no encuentre el usuario retorna false y estado 404
                if (userToUpdate == null)
                {
                    return NotFound(new { isSuccess = false, message = $"Usuario con ID {objeto.userId} no encontrado." });
                }
                //En caso de que si encuentre el usuario, actualiza el estado del usuario en la instancia.
                userToUpdate.EmailState = objeto.emailstate;
                //Actualiza en la base de datos
                _DB_CEA_Context.CeaUsers.Update(userToUpdate);
                //Guarda cambios en la base de datos
                await _DB_CEA_Context.SaveChangesAsync();
                //Si el proceso fue exitoso, retorna true y estado 200
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, message = "Estado del correo actualizado correctamente." });
            }
            catch (DbUpdateConcurrencyException)
            {
                //Si ocurre algún error en la interacción con la base de datos, retornar false y estado 409
                return StatusCode(StatusCodes.Status409Conflict, new { isSuccess = false, message = "Conflicto de concurrencia. El usuario fue modificado por otro proceso." });
            }
            catch (Exception ex)
            {
                //Si ocurre un error en el try, retorna false, estado 500 y el cuerpo del error.
                return StatusCode(StatusCodes.Status500InternalServerError, new { isSuccess = false, message = $"Ocurrió un error al actualizar el estado del correo: {ex.Message}" });
            }
        }

        //Establece un método Patch 
        [HttpPatch]
        //Establece la ruta para el metodo /api/Users/updatePass
        [Route("updatePass")]
        //Recibe del body un objeto de tipo UpdatePassDTO 
        public async Task<IActionResult> updatePass([FromBody] UpdatePassDTO objeto)
        {
            try
            {
                //Instancia un usuario de tipo CeaUsers, realiza la búsqueda en la base de datos basado en el Id entregado en UpdatePassDTO
                var userToUpdate = await _DB_CEA_Context.CeaUsers.FirstOrDefaultAsync(u => u.IdUser == objeto.userId);
                //En caso de que no encuentre el usuario ,retorna false y estado 404
                if (userToUpdate == null)
                {
                    return NotFound(new { isSuccess = false, message = $"Usuario no encontrado." });
                }
                //En caso de que encuentre el usuario, acutaliza la contraseña de la instancia
                userToUpdate.PassUser = _utilidades.encriptarSHA256(objeto.password);
                //Actualiza el usuario en la base de datos
                _DB_CEA_Context.CeaUsers.Update(userToUpdate);
                //Guarda cambios en la base de datos.
                await _DB_CEA_Context.SaveChangesAsync();

                //Si el proceso termina bien, retorna true y estado 200
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, message = "Contraseña modificada correctamente" });
            }
            catch (DbUpdateConcurrencyException)
            {
                //Si ocurre un error en la interacción con la base de datos retorna false y estado 409
                return StatusCode(StatusCodes.Status409Conflict, new { isSuccess = false, message = "Conflicto de concurrencia. El usuario fue modificado por otro proceso." });
            }
            catch (Exception ex)
            {
                //Si ocurre un error dentro del try, retorna false, estado 500 y el cuerpo del error.
                return StatusCode(StatusCodes.Status500InternalServerError, new { isSuccess = false, message = $"Ocurrió un error al actualizar la contraseña: {ex.Message}" });
            }
        }
    }
}
