using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CEA_API.Custom;
using CEA_API.Models;
using CEA_API.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using CEA_API.Interfaces;
using System;
using System.Threading.Tasks;

namespace CEA_API.Controllers
{
    //Establece la ruta para el controlador
    [Route("api/[controller]")]
    [ApiController]
    public class ValidateController : ControllerBase
    {
        //Instancia un objeto para la base de datos
        private readonly J54hfncyh4CeaContext _DB_CEA_Context;
        //Instancia un objeto para el manejo de los JWToken
        private readonly Utilidades _utilidades;
        //Instancia un objeto para el envío de correos electrónico
        private readonly IEmailService _emailService;
        //Instancia un objeto para guardar los códigos enviados en el caché
        private readonly IMemoryCache _cache;

        //Establece el controlador del controlador
        public ValidateController(J54hfncyh4CeaContext DB_CEA_Context, Utilidades utilidades, IEmailService emailService, IMemoryCache cache)
        {
            _DB_CEA_Context = DB_CEA_Context;
            _utilidades = utilidades;
            _emailService = emailService;
            _cache = cache;
        }

        //Establece un método Post
        [HttpPost]
        //Establece que para acceder al método deberá tener token de autorización 
        [Authorize]
        //Establece la ruta para el método.
        [Route("SendVerificationCode")]
        //Recibe un objeto de tipo EmailsDTO
        public async Task<IActionResult> SendVerificationCode([FromBody] EmailsDTO objeto)
        {
            //En caso de que el objeto de tipo EmailsDTO sea vacío o nulo, retorna false y estado 400
            if (string.IsNullOrWhiteSpace(objeto.Value))
            {
                return BadRequest(new { isSuccess = false, message = "El valor de búsqueda (nombre de usuario o email) no puede estar vacío." });
            }
            try
            {
                //Realiza la búsqueda del usuario en la base de datos igualando el valor "value" al usuario o al correo
                var user = await _DB_CEA_Context.CeaUsers
                                .Where(u => u.NameUser == objeto.Value || u.EmailUser == objeto.Value)
                                //En caso de que haya un usuario con ese correo o usuario
                                .Select(u => new
                                {
                                    u.IdUser,
                                    u.NameUser,
                                    u.EmailUser
                                })
                                .FirstOrDefaultAsync();
                //En caso de que no encuentre el usuario, retorna false y estado 404
                if (user == null)
                {
                    return NotFound(new { isSuccess = false, message = "No se encontró un usuario con la información proporcionada." });
                }
                //En caso de que el correo del usuario encontrado esté vacío, retorna false y estado 400
                if (string.IsNullOrWhiteSpace(user.EmailUser))
                {
                    return BadRequest(new { isSuccess = false, message = "El usuario encontrado no tiene un correo electrónico válido para enviar el código." });
                }
                //En caso de que encuentre el usuario y el correo no esté vacío, genera el código de 6 digitos
                string DynamicCode = GenerateSixDigitCode();
                //Establece el tiempo de expiración del código
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(15));
                //Almacena el código en el caché del api
                _cache.Set($"DynamicValidationCode_{user.IdUser}", DynamicCode, cacheEntryOptions);
                //Establece el nombre del usuario para el cuerpo del correo
                string userNameForEmail = string.IsNullOrWhiteSpace(user.NameUser) ? "Estimado usuario" : user.NameUser;
                //Establece el asunto del correo.
                string emailSubject = "Clave dinámica para restablecer acceso al CEA";
                //Establece el HTML del correo
                string htmlEmailBody = $@"
                <!DOCTYPE html>
                <html lang='es'>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>{emailSubject}</title>
                    <style>
                        body {{
                            font-family: Arial, sans-serif;
                            line-height: 1.6;
                            color: #333333;
                            background-color: #f4f4f4;
                            margin: 0;
                            padding: 20px;
                        }}
                        .container {{
                            max-width: 600px;
                            margin: 20px auto;
                            background-color: #ffffff;
                            padding: 30px;
                            border-radius: 8px;
                            box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                            border-top: 5px solid #007bff; /* Color primario de tu aplicación */
                        }}
                        .header {{
                            text-align: center;
                            padding-bottom: 20px;
                            border-bottom: 1px solid #eeeeee;
                        }}
                        .header h1 {{
                            margin: 0;
                            color: #007bff;
                        }}
                        .content {{
                            padding: 20px 0;
                        }}
                        .code-box {{
                            background-color: #e9ecef;
                            border: 1px solid #ced4da;
                            padding: 15px;
                            text-align: center;
                            font-size: 24px;
                            font-weight: bold;
                            color: #dc3545; /* Un color distintivo para el código */
                            border-radius: 4px;
                            margin: 20px 0;
                        }}
                        .footer {{
                            text-align: center;
                            padding-top: 20px;
                            border-top: 1px solid #eeeeee;
                            font-size: 12px;
                            color: #666666;
                        }}
                        .button {{
                            display: inline-block;
                            background-color: #007bff;
                            color: #ffffff;
                            padding: 10px 20px;
                            text-decoration: none;
                            border-radius: 5px;
                            margin-top: 20px;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Clave dinámica CEA</h1>
                        </div>
                        <div class='content'>
                            <p>Hola <strong>{userNameForEmail}</strong>,</p>
                            <p>Recientemente te fue solicitada la clave dinámica para restablecer el acceso al CEA (Centro de automatizaciones). Por favor, utiliza el siguiente código para completar el proceso:</p>
                            <div class='code-box'>
                                {DynamicCode}
                            </div>
                            <p>Este código es válido por <strong>15 minutos</strong>. Si no solicitaste la clave dinámica por favor comunicate con el soporte de TI.</p>
                            <p>Por motivos de seguridad, no compartas este código con nadie.</p>
                            <p>Gracias por usar nuestros servicios.</p>
                        </div>
                        <div class='footer'>
                            <p>&copy; {DateTime.Now.Year} Equipo de Soporte CEA.</p> <br/>
                            <p>EPAOH - Establecimiento Público Aeropuerto Olaya Herrera</p>
                        </div>
                    </div>
                </body>
                </html>";
                //Realiza el envío del correo electrónico
                await _emailService.SendEmailWithHtmlAsync(user.EmailUser, emailSubject, htmlEmailBody);
                //Si el proceso termina de manera exitosa, retorna true y estado 200
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, message = "Código de verificación enviado a tu correo. Revisa tu bandeja de entrada." });
            }
            catch (ApplicationException aex)
            {
                //Si ocurre algún error en la interacción con el envío del correo electrónico, retorna false, estado 500 y el cuerpo del error
                Console.WriteLine($"Error al enviar email en RestorePasswordController: {aex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { isSuccess = false, message = $"Error de envío de correo: {aex.Message}" });
            }
            catch (Exception ex)
            {
                //En caso de que ocurra un error dentro del try, retorna false, estado 500 y el cuerpo del error
                Console.WriteLine($"Error general en RestorePasswordController (SendVerificationCode): {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { isSuccess = false, message = $"Ocurrió un error al procesar tu solicitud: {ex.Message}" });
            }
        }
        //Establece el metodo para la generación del código de 5 dígitos 
        private string GenerateSixDigitCode()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        //Establece un método Post
        [HttpPost]
        //Establece la ruta para el método /api/Validate/VerifyVerificationCode
        [Route("VerifyVerificationCode")]
        //Recibe un objeto de tipo VerifyDynamicCodeDTO 
        public async Task<IActionResult> VerifyVerificationCode([FromBody] VerifyDynamicCodeDTO verifyCodeDto)
        {
            //En caso de que el objeto de tipo VerifyDynamicCodeDTO sea nulo o vacío retorna false y estado 400
            if (string.IsNullOrWhiteSpace(verifyCodeDto.Value))
            {
                return BadRequest(new StandardApiResponseDTO { IsSuccess = false, Message = "El valor de búsqueda (nombre de usuario o email) no puede estar vacío." });
            }

            try
            {
                //Realiza la búsqueda del usuario por correo electrónico u correo
                var user = await _DB_CEA_Context.CeaUsers
                               .Where(u => u.NameUser == verifyCodeDto.Value || u.EmailUser == verifyCodeDto.Value)
                               .FirstOrDefaultAsync();
                //En caso de que no encentre el usuario, retorna false y estado 404
                if (user == null)
                {
                    return NotFound(new StandardApiResponseDTO { IsSuccess = false, Message = "No se encontró un usuario con la información proporcionada." });
                }
                //En caso de que si encuentre el usuario, realiza la búsqueda del código en el caché de la api.
                //Si no encuentra el código quiere decir que expiró. Retorna false y estado 400
                if (!_cache.TryGetValue($"DynamicValidationCode_{user.IdUser}", out string cachedCode))
                {
                    return BadRequest(new StandardApiResponseDTO { IsSuccess = false, Message = "El código de verificación ha expirado o no es válido. Por favor, solicita uno nuevo." });
                }

                //En caso de que el código entregado por el usuario sea diferente al encontrado, retorna false y estado 400
                if (cachedCode != verifyCodeDto.Code)
                {
                    return BadRequest(new StandardApiResponseDTO { IsSuccess = false, Message = "El código de verificación es incorrecto." });
                }
                //En caso de que el código entregado por el usuario sea correcto, elimina el código del caché
                _cache.Remove($"DynamicValidationCode_{user.IdUser}");
                //Genera token de sesión
                string generatedToken = _utilidades.generarJWT(user);
                //Retorna true, el token de sesión, el id del usuario, el rol del usuario y estado 200
                return StatusCode(StatusCodes.Status200OK, new VerificationSuccessResponseDTO
                {
                    IsSuccess = true,
                    Token = generatedToken, 
                    IdUser = user.IdUser,
                    IdUsersRolUser = user.IdUsersRolUser
                });

            }
            catch (Exception ex)
            {
                //En caso de un error dentro del try, retorna false, estado 500 y el cuerpo del error.
                Console.WriteLine($"Error general en RestorePasswordController (VerifyVerificationCode): {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return StatusCode(StatusCodes.Status500InternalServerError, new StandardApiResponseDTO { IsSuccess = false, Message = $"Ocurrió un error al verificar el código: {ex.Message}" });
            }
        }
    }
}
