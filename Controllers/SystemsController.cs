using CEA_API.Custom;
using CEA_API.Models;
using CEA_API.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CEA_API.Controllers
{
    //Establece la ruta principal para el controlador /api/Systems
    [Route("api/[controller]")]
    //Establece la autorización para el controlador
    [Authorize]
    [ApiController]
    public class SystemsController : ControllerBase
    {
        //Instancia un objeto para la base de datos
        private readonly J54hfncyh4CeaContext _DB_CEA_Context;

        //Establece en constructor para el contructor.
        public SystemsController(J54hfncyh4CeaContext DB_CEA_Context)
        {
            _DB_CEA_Context = DB_CEA_Context;
        }

        //Establece un método GET
        [HttpGet]
        //Establece la ruta para el método /api/Systems/ListSystems
        [Route("ListSystems")]
        public async Task<IActionResult> ListPermissions()
        {
            try
            {
                //Instancia un objeto para almacenar los sistemas encontrados
                var systems = await _DB_CEA_Context.CeaSystems
                                        //Realiza la selección de los datos del sistema que se quieren mostrar.
                                        .Select(u => new SystemsDTO
                                        {
                                            IdSystem = u.IdSystem,
                                            NameSystem = u.NameSystem,
                                            LinkSystems = u.LinkSystems
                                        })
                                        //Lista los sistemas encontrados.
                                        .ToListAsync();

                //Si no encuentra sistemas retorna false y estaso 404
                if (systems == null || !systems.Any())
                {
                    return NotFound(new { isSuccess = false, message = "No se encontraron permisos" });
                }
                //En caso de que si encuentre sistemas returna true, estado 200 y la lista de los sistemas.
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, systems });
            }
            catch (Exception ex)
            {
                //En caso de que se presente un error en el try, retorna false, estado 500 y el cuerpo del error. 
                return StatusCode(StatusCodes.Status500InternalServerError, new { isSuccess = false, message = $"Ocurrió un error al obtener la lista de Permisos: {ex.Message}" });
            }
        }

        //Establece un metodo Post
        [HttpPost]
        //Establece la ruta para el metodo /api/Systems/register
        [Route("register")]
        //Recibe un objeto de tipo SystemsDTO 
        public async Task<IActionResult> register([FromBody] SystemsDTO objeto)
        {
            //Instancia un nuevo objeto de tipo CeaSystem
            var SystemModel = new CeaSystem
            {
                //Entrega al objeto de tipo CeaSystem los datos entregados por el usuario
                NameSystem = objeto.NameSystem,
                LinkSystems = objeto.LinkSystems
            };

            try
            {
                //Agrega el sistema a la base de datos
                await _DB_CEA_Context.CeaSystems.AddAsync(SystemModel);
                //Guarda cambios en la base de datos
                await _DB_CEA_Context.SaveChangesAsync();

                //En caso de que retorne el id del sistema, devuelve true, estado 201 y el id del sistema 
                if (SystemModel.IdSystem != 0)
                {
                    return StatusCode(StatusCodes.Status201Created, new { isSuccess = true, SystemId = SystemModel.IdSystem, message = "Sistema registrado correctamente." });
                }
                else
                {
                    /*En caso de que no devuelva el id del sistema, retorna false y estado 500*/
                    return StatusCode(StatusCodes.Status500InternalServerError, new { isSuccess = false, message = "El registro del sistema falló." });
                }
            }
            catch (Exception ex)
            {
                //En caso de que se presente un error en el try, retorna false, estado 500 y el cuerpo del mensaje.
                return StatusCode(StatusCodes.Status500InternalServerError, new { isSuccess = false, message = $"Ocurrió un error durante el registro: {ex.Message}" });
            }
        }

        //Establece un método PUT
        [HttpPut]
        //Establece la ruta del método /api/Systems/update/id
        [Route("update/{id:int}")]
        //Recibe el id y un objeto de tipo SystemsDTO
        public async Task<IActionResult> UpdateSystem(int id, [FromBody] SystemsDTO objeto)
        {
            /*ModelState representa el estado del modelo de datos. En caso de ser no estar válido 
            retorna estado 400*/
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                //Realiza la búsqueda del sistema en la base de datos por id
                var systemToUpdate = await _DB_CEA_Context.CeaSystems.FirstOrDefaultAsync(u => u.IdSystem == id);

                //En caso de que no encuentre el sistema, retorna false y el estado 404
                if (systemToUpdate == null)
                {
                    return NotFound(new { isSuccess = false, message = $"Sistema con ID {id} no encontrado." });
                }

                //En caso de que si encuentre el sistema, actualiza los atributos del sistema en base al objeto entregado por el usuario
                systemToUpdate.NameSystem = objeto.NameSystem;
                systemToUpdate.LinkSystems = objeto.LinkSystems;

                //Actualiza el sistema en la base de datos
                _DB_CEA_Context.CeaSystems.Update(systemToUpdate);
                //Guarda cambios en la base de datos.
                await _DB_CEA_Context.SaveChangesAsync();

                //Si el proceso culmina bien, retorna true, mensaje de validación y estado 200
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, message = "Sistema actualizado exitosamente." });
            }
            catch (DbUpdateConcurrencyException)
            {
                //En caso de que se presente un error al momento de actualizar en la base de datos, retorna false y estado 409 
                return StatusCode(StatusCodes.Status409Conflict, new { isSuccess = false, message = "Conflicto de concurrencia. El sistema fue modificado por otro proceso." });
            }
            catch (Exception ex)
            {
                //En caso de que se presente un error dentro del try, retorna false y estado 500
                return StatusCode(StatusCodes.Status500InternalServerError, new { isSuccess = false, message = $"Ocurrió un error al actualizar el sistema: {ex.Message}" });
            }
        }

        //Establece un método Delete
        [HttpDelete]
        //Establece la ruta para el método /api/Systems/delete/id
        [Route("delete/{id:int}")]
        //Recibe el id del sistema a eliminar.
        public async Task<IActionResult> deleteSystem(int id)
        {
            try
            {
                //Instancia un ibjeto de tipo CeaSystems para la búsqueda del sistema en la base de datos.
                var systemToDelete = await _DB_CEA_Context.CeaSystems.FirstOrDefaultAsync(u => u.IdSystem == id);
                //En caso de que no encuentre el sistema, retorna false y estado 404
                if (systemToDelete == null)
                {
                    return NotFound(new { isSuccess = false, message = $"Sistema con id {id} no encontrado." });
                }

                //En caso de que si encuentre el sistema con el id entregado, realiza la remoción del mismo en la base de datos.
                _DB_CEA_Context.CeaSystems.Remove(systemToDelete);
                //Guarda cambios en la base de datos.
                await _DB_CEA_Context.SaveChangesAsync();

                //Si el proceso culmina correctamente, retorna true y estado 200
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, message = "Sistema eliminado exitosamente." });
            }
            catch (Exception ex)
            {
                //En caso de que se presente un error dentro del try, retorna false, estado 500 y el cuerpo del error.
                return StatusCode(StatusCodes.Status500InternalServerError, new { isSuccess = false, message = $"Ocurrió un error al eliminar el sistema: {ex.Message}" });
            }
        }
    }
}
