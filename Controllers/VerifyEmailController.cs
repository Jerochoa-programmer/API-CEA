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
    //Establece la ruta para el controlador /api/VerifyEmail
    [Route("api/[controller]")]
    //Establece que para acceder al controlador se debe de estar autorizado con jwt 
    [Authorize]
    [ApiController]
    public class VerifyEmailController : ControllerBase
    {
        //Instancia un objeto para la base de datos. 
        private readonly J54hfncyh4CeaContext _DB_CEA_Context;
        //Instancia un objeto para el envío de correos electrónicos
        private readonly IEmailService _emailService;
        //Instancia un objeto para el almacenamiento del código en el caché
        private readonly IMemoryCache _cache;

        //Establece el constructor del controlador
        public VerifyEmailController(J54hfncyh4CeaContext DB_CEA_Context, IEmailService emailService, IMemoryCache cache)
        {
            _DB_CEA_Context = DB_CEA_Context;
            _emailService = emailService;
            _cache = cache;
        }


        //Establece un método Post
        [HttpPost]
        //Establece la ruta pata el método /api/VerifyEmail/SendVerificationCode
        [Route("SendVerificationCode")]
        //Recibe un objeto de tipo EmailsDTO
        public async Task<IActionResult> SendVerificationCode([FromBody] EmailsDTO objeto)
        {
            //En caso de que el objeto entregado sea vacío o nulo, retorna false y estado 400
            if (string.IsNullOrWhiteSpace(objeto.Value))
            {
                return BadRequest(new { isSuccess = false, message = "El valor de búsqueda (nombre de usuario o email) no puede estar vacío." });
            }
            try
            {
                //Realiza la búsqueda del usuario en la base de datos.
                var user = await _DB_CEA_Context.CeaUsers
                                .Where(u => u.NameUser == objeto.Value || u.EmailUser == objeto.Value)
                                .Select(u => new
                                {
                                    u.IdUser,
                                    u.NameUser,
                                    u.EmailUser
                                })
                                .FirstOrDefaultAsync();
                //En caso de que no encuentre el usuario en la base de datos, retorna false y estado 404
                if (user == null)
                {
                    return NotFound(new { isSuccess = false, message = "No se encontró un usuario con la información proporcionada." });
                }
                //En caso de encontrar el usuario pero que el correo esté vacío o nulo, retorna false y estado 400
                if (string.IsNullOrWhiteSpace(user.EmailUser))
                {
                    return BadRequest(new { isSuccess = false, message = "El usuario encontrado no tiene un correo electrónico válido para enviar el código." });
                }
                //En caso de que encuentre el usuario y tenga correo, genera el código de 6 digitos
                string validationCode = GenerateSixDigitCode();
                //Establece el tiempo de expiración del código.
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(15));
                //Almacena el código en el cahcé de la api
                _cache.Set($"EmailValidationCode_{user.IdUser}", validationCode, cacheEntryOptions);
                //Establece el nombre del usuario para el cuerpo del correo
                string userNameForEmail = string.IsNullOrWhiteSpace(user.NameUser) ? "Estimado usuario" : user.NameUser;
                //Establece el asunto del correo.
                string emailSubject = "Código de verificación para validar correo registrado en el CEA";
                //Establece el html del correo 
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
                            <h1>Código de validación CEA</h1>
                        </div>
                        <div class='content'>
                            <p>Hola <strong>{userNameForEmail}</strong>,</p>
                            <p>Este correo es para notificarte del código para validar tu correo electrónico. Por favor, utiliza el siguiente código para completar el proceso:</p>
                            <div class='code-box'>
                                {validationCode}
                            </div>
                            <p>Este código es válido por <strong>10 minutos</strong>. Si no solicitaste el registro en el sistema, puedes ignorar este correo.</p>
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
                //Realiza el envío del correo
                await _emailService.SendEmailWithHtmlAsync(user.EmailUser, emailSubject, htmlEmailBody);
                //Si el envío del correo fue exitoso, retorna true y estado 200
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, message = "Código de verificación enviado a tu correo. Revisa tu bandeja de entrada." });
            }
            catch (ApplicationException aex)
            {
                //Si se presenta un error en la interacción del envío de correo, retorna false, estado 500 y el cuerpo del error
                Console.WriteLine($"Error al enviar email en RestorePasswordController: {aex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { isSuccess = false, message = $"Error de envío de correo: {aex.Message}" });
            }
            catch (Exception ex)
            { 
                //Si se presenta algún error dentro del try, retorna false, estado 500 y el cuerpo del error
                Console.WriteLine($"Error general en RestorePasswordController (SendVerificationCode): {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { isSuccess = false, message = $"Ocurrió un error al procesar tu solicitud: {ex.Message}" });
            }
        }

        //Establece un método Post
        [HttpPost]
        //Establece la ruta para el método /api/VerifyEmail/VerifyVerificationCode
        [Route("VerifyVerificationCode")]
        //Recibe del body un objeto de tipo VerifyCodeDTO
        public IActionResult VerifyVerificationCode([FromBody] VerifyCodeDTO verifyCodeDto)
        {
            //En caso de que el objeto sea nulo, tenga espacios vacíos o el id que contenga sea 0, retorna false y estado 400
            if (verifyCodeDto == null || verifyCodeDto.UserId == 0 || string.IsNullOrWhiteSpace(verifyCodeDto.Code))
            {
                return BadRequest(new { isSuccess = false, message = "Se requieren el ID de usuario y el código de verificación." });
            }

            try
            {
                //Realiza la búsqeuda del código en el caché, en caso de no encontrár código, retorna false y estado 200
                if (!_cache.TryGetValue($"EmailValidationCode_{verifyCodeDto.UserId}", out string cachedCode))
                {
                    return Ok(new { isSuccess = false, message = "El código de verificación ha expirado o no es válido. Por favor, solicita uno nuevo." });
                }
                //En caso de que si encuentre el código pero sea diferente del código entregado por el usuario, retorna estado 200 y false.
                if (cachedCode != verifyCodeDto.Code)
                {
                    return Ok(new { isSuccess = false, message = "El código de verificación es incorrecto." });
                }
                //En caso de que el código sea correcto, elimina el código del caché
                _cache.Remove($"EmailValidationCode_{verifyCodeDto.UserId}");
                //Retorna true y estado 200
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, message = "Código verificado exitosamente. Ahora tienes acceso al sistema." });
            }
            catch (Exception ex)
            {
                //En caso de que exista un error dentro del try, retornas false, estado 500 y el cuerpo del error.
                Console.WriteLine($"Error general en RestorePasswordController (VerifyVerificationCode): {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { isSuccess = false, message = $"Ocurrió un error al verificar el código: {ex.Message}" });
            }
        }
        //Establece el método para la generación del código de 6 digitos.
        private string GenerateSixDigitCode()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }
    }
}
