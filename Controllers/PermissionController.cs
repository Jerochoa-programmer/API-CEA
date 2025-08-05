using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CEA_API.Custom;
using CEA_API.Models;
using CEA_API.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Linq;

namespace CEA_API.Controllers
{
    
    //Establece la ruta /api/Permission
    [Route("api/[controller]")]
    //Establece que deberá de ser autenticado para usar este controlador.
    [Authorize]
    [ApiController]
    public class PermissionController : ControllerBase
    {
        //Instancia un objeto para la base de datos 
        private readonly J54hfncyh4CeaContext _DB_CEA_Context;

        //Establece el constructor del controlador.
        public PermissionController(J54hfncyh4CeaContext DB_CEA_Context)
        {
            _DB_CEA_Context = DB_CEA_Context;
        }

        //Establece un metodo POST
        [HttpPost]
        //Establece la ruta "api/permission/register"
        [Route("register")]
        public async Task<IActionResult> register([FromBody] PermissionDTO objeto)
        {
            //Instancia un objeto para el permiso
            var PermissionModel = new CeaPermission
            {
                //Entrega al objeto los valores de codigo y sistema
                CodePermission = objeto.CodePermission,
                IdSystemPermission = objeto.IdSystemPermission
            };
            try
            {   
                //Agrega el permiso instanciado a la base de datos
                await _DB_CEA_Context.CeaPermissions.AddAsync(PermissionModel);
                //Guarda cambios en la base de datos.
                await _DB_CEA_Context.SaveChangesAsync();

                
                //En caso de retornarse el id del permiso devuelve true
                if (PermissionModel.IdPermission != 0)
                {
                    return StatusCode(StatusCodes.Status201Created, new { isSuccess = true, userId = PermissionModel.IdPermission, message = "Permiso registrado correctamente." });
                }
                else
                {
                    //En caso de no retornarse el id del permiso, significa que no se grabó y retorna false
                    return StatusCode(StatusCodes.Status500InternalServerError, new { isSuccess = false, message = "El registro del permiso falló." });
                }
            }
            //En caso de presentarse un error en el try, devuelve false y el error.
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { isSuccess = false, message = $"Ocurrió un error durante el registro: {ex.Message}" });
            }
        }

        //Establece un metodo GET
        [HttpGet]
        //Establece la ruta /api/permission/ListPermissions
        [Route("ListPermissions")]
        public async Task<IActionResult> ListPermissions()
        {
            try
            {
                //Instancia un objeto para los permisos. 
                var permissions = await _DB_CEA_Context.CeaPermissions
                                        //Incluye la tabla de sistemas 
                                        .Include(p => p.IdSystemPermissionNavigation)
                                        //Selecciona los valores necesarios a visualizar en la lista.
                                        .Select(u => new PermissionDTO
                                        {
                                            IdPermission = u.IdPermission,
                                            CodePermission = u.CodePermission,
                                            IdSystemPermission = u.IdSystemPermission,
                                            //Establece el atributo de nombre de sistema basado en la clave foranea en la tabla permissos. 
                                            NameSystem = u.IdSystemPermissionNavigation != null ? u.IdSystemPermissionNavigation.NameSystem : null
                                        })
                                        //Lista los permisos encontrados.
                                        .ToListAsync();
                //En caso de no encontrar permisos retorna false.
                if (permissions == null || !permissions.Any())
                {
                    return NotFound(new { isSuccess = false, message = "No se encontraron permisos" });
                }
                //En caso de encontrar permisos retorna true.
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, permissions });
            }
            catch (Exception ex)
            {
                //En caso de capturar un error en el try, devuelve false y el mensaje del error. 
                return StatusCode(StatusCodes.Status500InternalServerError, new { isSuccess = false, message = $"Ocurrió un error al obtener la lista de Permisos: {ex.Message}" });
            }
        }

        //Establece un metodo PUT
        [HttpPut]
        //Establece la ruta /api/update/id
        [Route("update/{id:int}")]
        public async Task<IActionResult> UpdatePermission(int id, [FromBody] PermissionDTO objeto)
        {
            /*ModelState representa el estado del modelo de datos. En caso de ser no estar válido 
            retorna estado 400*/
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {   
                //Instancia un objeto para el buscar el permiso que se va a modificar. 
                var permissionUpdate = await _DB_CEA_Context.CeaPermissions.FirstOrDefaultAsync(u => u.IdPermission == id);

                //Si no encuentra el permiso retorna false y estado 404
                if (permissionUpdate == null)
                {
                    return NotFound(new { isSuccess = false, message = $"Permiso con id {id} no encontrado" });
                }

                /*En caso de encontrar el permiso a modificar, actualiza el código y el id del sistema en
                la instancia del permiso*/
                permissionUpdate.CodePermission = objeto.CodePermission;
                permissionUpdate.IdSystemPermission = objeto.IdSystemPermission;

                //Actualiza el permiso en la base de datos.
                _DB_CEA_Context.CeaPermissions.Update(permissionUpdate);
                //Guarda cambios en la base de datos. 
                await _DB_CEA_Context.SaveChangesAsync();

                //Si el proceso fue exitoso retornar true y el estado 200
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, message = "Permiso actualizado exitosamente" });

            }
            catch (DbUpdateConcurrencyException)
            {
                /*En caso de presentarse un error en la actualización de la base de datos 
                retorna false y estado 409 */
                return StatusCode(StatusCodes.Status409Conflict, new { isSuccess = false, message = "Conflicto de concurrencia. El permiso fue modificado por otro proceso." });
            }
            catch (Exception ex)
            {
                //En caso de presentarse un error dentro del try retorna false y estado 500
                return StatusCode(StatusCodes.Status500InternalServerError, new { isSuccess = false, message = $"Ocurrió un error al actualizar el permiso: {ex.Message}" });
            }
        }

        //Establece un metodo DELETE
        [HttpDelete]
        //Establece la ruta api/permissions/delete/id
        [Route("delete/{id:int}")]
        public async Task<IActionResult> deletePermission(int id)
        {
            try
            {
                //Instancia un objeto para la búsqueda del permiso a eliminar.
                var permissionToDelete = await _DB_CEA_Context.CeaPermissions.FirstOrDefaultAsync(u => u.IdPermission == id);
                //En caso de no encontrar el permiso retorna false y estado 404
                if (permissionToDelete == null)
                {
                    return NotFound(new { isSuccess = false, message = $"Permiso con id {id} no encontrado." });
                }

                //En caso de encontrar el permiso, se elimina de la base de datos el permiso enconntrado
                _DB_CEA_Context.CeaPermissions.Remove(permissionToDelete);
                //Guarda cambios en la base de datos. 
                await _DB_CEA_Context.SaveChangesAsync();

                //En caso de que el proceso sea exitoso, retorna true y el estado 200
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, message = "Permiso eliminado exitosamente." });
            }
            catch (Exception ex)
            {
                //En caso de presentarse un error dentro del try, retorna false y estado 500
                return StatusCode(StatusCodes.Status500InternalServerError, new { isSuccess = false, message = $"Ocurrió un error al eliminar el permiso: {ex.Message}" });
            }
        }

        //Establece un metodo POST
        [HttpPost]
        //Establece la ruta api/permission/assignPermissions
        [Route("assignPermissions")] 
        public async Task<IActionResult> AssignPermissions([FromBody] List<PermissionGrantedDTO> assignments)
        {
            //En caso de que se entregue una lista vacía retorna falso y estado 400
            if (assignments == null || !assignments.Any())
            {
                return BadRequest(new { isSuccess = false, message = "No se proporcionaron asignaciones de permisos." });
            }

            
            //Instancia un objeto para el id de los usuarios a los que se le asignarán permisos. 
            var distinctUserIds = assignments.Select(a => a.IdPermissionGrantedUser).Distinct().ToList();
            //Lista los id del os permisos a asignar.
            var distinctPermissionIds = assignments.Select(a => a.IdPermissionGrantedPermission).Distinct().ToList();


            //Realiza la búsqueda del usuario al que se le asignarán los permisos.
            var existingUsers = await _DB_CEA_Context.CeaUsers
                                                     .Where(u => distinctUserIds.Contains(u.IdUser))
                                                     .Select(u => u.IdUser)
                                                     .ToListAsync();
            //Realiza la búsqueda de los permisos que serán asignados.
            var existingPermissions = await _DB_CEA_Context.CeaPermissions
                                                         .Where(p => distinctPermissionIds.Contains(p.IdPermission))
                                                         .Select(p => p.IdPermission)
                                                         .ToListAsync();

            //Instancia una lista para los usuarios que no se encontraron. 
            var missingUsers = distinctUserIds.Except(existingUsers).ToList();
            //Instancia una lista para los permisos que no fueron encontrados. 
            var missingPermissions = distinctPermissionIds.Except(existingPermissions).ToList();

            /*En caso de que hayan usuarios o permisos que no se encontraron 
             retorna los usuarios o permisos que no se encontraron y el metodo 404*/
            if (missingUsers.Any() || missingPermissions.Any())
            {
                var errorMessage = "Los siguientes IDs no existen: ";
                if (missingUsers.Any()) errorMessage += $"Usuarios: {string.Join(", ", missingUsers)}. ";
                if (missingPermissions.Any()) errorMessage += $"Permisos: {string.Join(", ", missingPermissions)}.";
                return NotFound(new { isSuccess = false, message = errorMessage });
            }

            /*En caso de que no hayan permisos o usuarios sin encontrar, instancia una lista de tipo 
             PermissionsGranted*/
            var permissionsGrantedToAdd = new List<CeaPermissionsGranted>();
            foreach (var assignment in assignments)
            {
                //Agrega cada una de las asignaciones a la lista.
                permissionsGrantedToAdd.Add(new CeaPermissionsGranted
                {
                    IdPermissionGrantedUser = assignment.IdPermissionGrantedUser,
                    IdPermissionGrantedPermission = assignment.IdPermissionGrantedPermission
                    
                });
            }

            try
            {
                //Consulta si alguna de las asignaciones que se van a realizar ya existen 
                var existingAssignments = await _DB_CEA_Context.CeaPermissionsGranteds
                    .Where(pg => distinctUserIds.Contains(pg.IdPermissionGrantedUser) && distinctPermissionIds.Contains(pg.IdPermissionGrantedPermission))
                    .ToListAsync();

                //Filtra las asignaciones para solo realizar las que no están registradas en la base de datos
                var newAssignmentsFiltered = permissionsGrantedToAdd
                    .Where(newPg => !existingAssignments.Any(
                        existingPg => existingPg.IdPermissionGrantedUser == newPg.IdPermissionGrantedUser && existingPg.IdPermissionGrantedPermission == newPg.IdPermissionGrantedPermission
                    ))
                    .ToList();

                /*En caso de que todas la asignaciones por realizar ya se encuentren en la base de datos, retorna 
                 true, estado 200 y un mensaje diciendo que los permisos ya estaban asignados.*/
                if (!newAssignmentsFiltered.Any())
                {
                    return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, message = "Todos los permisos ya estaban asignados a estos usuarios." });
                }

                /*En caso de que si hayan permisos que no estén asignados la usuario, los registra en la base de datos*/
                await _DB_CEA_Context.CeaPermissionsGranteds.AddRangeAsync(newAssignmentsFiltered);
                //Guarda cambios en la base de datos.
                await _DB_CEA_Context.SaveChangesAsync();

                //Si el proceso fue exitoso, retorna estado 201, true y un mensaje de exito.
                return StatusCode(StatusCodes.Status201Created, new
                {
                    isSuccess = true,
                    assignedCount = newAssignmentsFiltered.Count,
                    message = "Permisos asignados exitosamente."
                });
            }
            catch (Exception ex)
            {
                //En caso de que se presente un error en el try, devolverá false, estado 500 y el cuerpo del mensaje.
                return StatusCode(StatusCodes.Status500InternalServerError, new { isSuccess = false, message = $"Ocurrió un error al asignar permisos: {ex.Message}" });
            }
        }

        //Establece un método GET
        [HttpGet]
        //Establece la ruta /api/Permission/ListUserPermissions/id
        [Route("ListUserPermissions/{id:int}")]
        //Recibe un id
        public async Task<IActionResult> ListUserPermissions(int id)
        {
            try
            {
                /*Instancia una lista para los permisos asociados al usuario con el id entregado*/
                var permissions = await _DB_CEA_Context.CeaPermissionsGranteds
                                        //Busca todos los permisos asociados al id entregado
                                        .Where(u => u.IdPermissionGrantedUser == id)
                                        //Incluye la tabla de permisos
                                        .Include(pg => pg.IdPermissionGrantedPermissionNavigation)
                                        //Selecciona lo que se va a listar, id del usuario, id del permiso y código del permiso
                                        .Select(u => new PermissionGrantedWithSystemDTO
                                        {
                                            IdPermissionGrantedUser = u.IdPermissionGrantedUser,
                                            IdPermissionGranted = u.IdPermissionGranted,
                                            CodePermission = u.IdPermissionGrantedPermissionNavigation.CodePermission
                                        })
                                        //Lista los permisos.
                                        .ToListAsync();

                //Si no encuentra permisos asociados al usuario retorna false y estado 200
                if (permissions == null || !permissions.Any())
                {
                    return Ok(new { isSuccess = false, message = "No se encontraron permisos" });
                }

                //En caso de que si encuentre los permisos retornar estado 200, true y la lista de permisos.
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, permissions });
            }
            catch (Exception ex)
            {
                //En caso de que se presente algún error en el try se retornará estado 500, false y el cuerpo del error
                return StatusCode(StatusCodes.Status500InternalServerError, new { isSuccess = false, message = $"Ocurrió un error al obtener la lista de Permisos: {ex.Message}" });
            }
        }

        //Establece un método POST
        [HttpPost] 
        //Establece la ruta /api/Permission/removePermissionsGranted
        [Route("removePermissionsGranted")]
        //Recibe del body un objeto de tipo DeletePermissionGrantedDTO
        public async Task<IActionResult> removePermissionsGranted([FromBody] DeletePermissionGrantedDTO request)
        {
            //Si el objeto entregado es vació o nulo retorna false y estado 400
            if (request == null || !request.PermissionGrantedIds.Any())
            {
                return BadRequest(new { isSuccess = false, message = "No se proporcionaron IDs de asignaciones de permisos para revocar." });
            }

            try
            {
                //Instancia una lista para los permisos asignados a eliminar
                //Busca los permisos a eliminar en la base de datos.
                var permissionsToRevoke = await _DB_CEA_Context.CeaPermissionsGranteds
                                                                .Where(pg => request.PermissionGrantedIds.Contains(pg.IdPermissionGranted))
                                                                .ToListAsync();

                //En caso de que no encuentre ningun permiso para eliminar en la base de datos retorna 404
                if (!permissionsToRevoke.Any())
                {
                    return NotFound(new { isSuccess = false, message = "Ninguno de los permisos asignados especificados fue encontrado o ya fue removido." });
                }

                /*En caso de que si encuentre los permisos asignados por eliminar, realiza la remoción de los 
                mismoa en la base de datos*/
                _DB_CEA_Context.CeaPermissionsGranteds.RemoveRange(permissionsToRevoke);
                //Guarda cambios en la base de datos.
                await _DB_CEA_Context.SaveChangesAsync();
                //Si el proceso fue exitoso, retornar 200, true y un mensaje de notificación.
                return Ok(new { isSuccess = true, message = $"Se han removido {permissionsToRevoke.Count} permisos asignados exitosamente." });
            }
            catch (Exception ex)
            {
                // En caso de que se presente un error en el try, retorna false, estado 500 y el cuerpo del error-
                Console.WriteLine($"Error al revocar múltiples permisos asignados: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { isSuccess = false, message = $"Ocurrió un error interno del servidor al remover permisos: {ex.Message}" });
            }
        }
    }
}
