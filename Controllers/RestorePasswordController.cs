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
    //Establece la ruta principal al controlador /api/RestorePassword
    [Route("api/[controller]")]
    [ApiController]
    public class RestorePasswordController : ControllerBase
    {
        //Instancia la base de datos
        private readonly J54hfncyh4CeaContext _DB_CEA_Context;
        //Instancia utilidades para la generación de Tokens
        private readonly Utilidades _utilidades;
        //Instancia IEmailServide para el envío de correos electronicos 
        private readonly IEmailService _emailService; 
        //Instancia IMemoryCache para almacenare los código de validación en el caché de la api
        private readonly IMemoryCache _cache; 

        //Establece el constructor del controlador.
        public RestorePasswordController(J54hfncyh4CeaContext DB_CEA_Context, Utilidades utilidades, IEmailService emailService, IMemoryCache cache)
        {
            _DB_CEA_Context = DB_CEA_Context;
            _utilidades = utilidades;
            _emailService = emailService; 
            _cache = cache;
        }

        //Establece un método POST
        [HttpPost]
        //Establece la ruta /api/RestorePassword/SendVerificationCode
        [Route("SendVerificationCode")]
        //Recibe un objeto de tipo EmailsDTO
        public async Task<IActionResult> SendVerificationCode([FromBody] EmailsDTO userValue)
        {
            //En caso de que el objeto userValue sea nulo o vacío retorna 400
            if (string.IsNullOrWhiteSpace(userValue.Value))
            {
                return BadRequest(new { isSuccess = false, message = "El valor de búsqueda (nombre de usuario o email) no puede estar vacío." });
            }
            try
            {
                //Instancia un usuario
                var user = await _DB_CEA_Context.CeaUsers
                                //Realiza la búsqueda del usuario evaluando que el valor entregado sea igual a el usuario o el correo de los usuarios registrados.
                                .Where(u => u.NameUser == userValue.Value || u.EmailUser == userValue.Value)
                                //Registra la información necesitada del usuario. 
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
                /*En caso de que se encuentre el usuario pero este no cuente con correo en la base de datos,
                retorna false y estado 400*/
                if (string.IsNullOrWhiteSpace(user.EmailUser))
                {
                    return BadRequest(new { isSuccess = false, message = "El usuario encontrado no tiene un correo electrónico válido para enviar el código." });
                }

                /*En caso de que si se encuentre el usuario y tenga un correo registrado en la base de datos
                 genera un código de 6 digitos almacenado en verificationCode*/
                string verificationCode = GenerateSixDigitCode();

                //Establece el tiempo de expiración para el código
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(15));

                //Almacena el código en la base de datos.
                _cache.Set($"VerificationCode_{user.IdUser}", verificationCode, cacheEntryOptions);

                //Establece el nombre del usuario (Cómo será llamado en el correo)
                string userNameForEmail = string.IsNullOrWhiteSpace(user.NameUser) ? "Estimado usuario" : user.NameUser;
                //Establece el asunto del correo
                string emailSubject = "Código de verificación para restablecer tu contraseña en CEA";

                //Establece del HTML del correo.
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
                            <h1>Restablecimiento de Contraseña CEA</h1>
                        </div>
                        <div class='content'>
                            <p>Hola <strong>{userNameForEmail}</strong>,</p>
                            <p>Hemos recibido una solicitud para restablecer la contraseña de tu cuenta. Por favor, utiliza el siguiente código para completar el proceso:</p>
                            <div class='code-box'>
                                {verificationCode}
                            </div>
                            <p>Este código es válido por <strong>10 minutos</strong>. Si no solicitaste este restablecimiento de contraseña, puedes ignorar este correo.</p>
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

                //Realiza el envío del correo a la dirección del usuario
                await _emailService.SendEmailWithHtmlAsync(user.EmailUser, emailSubject, htmlEmailBody);
                //Si el proceso de envío terminó correctamente, retorna true y estado 200
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, message = "Código de verificación enviado a tu correo. Revisa tu bandeja de entrada." });
            }
            catch (ApplicationException aex)
            {
                //En caso de que se presente un error al momento de enviar el correo, retorna false, estado 500 y el cuerpo del error
                Console.WriteLine($"Error al enviar email en RestorePasswordController: {aex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { isSuccess = false, message = $"Error de envío de correo: {aex.Message}" });
            }
            catch (Exception ex)
            {
                //En caso de presentarse un error en el try, retorna false, estado 500 y el cuerpo del mensaje
                Console.WriteLine($"Error general en RestorePasswordController (SendVerificationCode): {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { isSuccess = false, message = $"Ocurrió un error al procesar tu solicitud: {ex.Message}" });
            }
        }

        //Establece un método para la generación del código
        private string GenerateSixDigitCode()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        //Establece un método POST
        [HttpPost]
        //Establece que solo se tendrá acceso a este método si se tiene un jwtToken
        [Authorize]
        //Establece la ruta para este metodo. 
        [Route("VerifyVerificationCode")]
        //Recibe un objeto de tipo VerifyCodeDTO
        public IActionResult VerifyVerificationCode([FromBody] VerifyCodeDTO verifyCodeDto)
        {
            //Si elk objeto está vacío, nulo o el UserId es 0, retorna false y estado 400
            if (verifyCodeDto == null || verifyCodeDto.UserId == 0 || string.IsNullOrWhiteSpace(verifyCodeDto.Code))
            {
                return BadRequest(new { isSuccess = false, message = "Se requieren el ID de usuario y el código de verificación." });
            }

            try
            {
                //Realiza la búsqueda del código en el caché de la api
                if (!_cache.TryGetValue($"VerificationCode_{verifyCodeDto.UserId}", out string cachedCode))
                {
                    //En caso de que no encuentre el código retorna false y estado 400
                    return BadRequest(new { isSuccess = false, message = "El código de verificación ha expirado o no es válido. Por favor, solicita uno nuevo." });
                }

                //En caso de que si encuentre el código pero no sea igual al entregado por el usuario, retorna false y estado 400 
                if (cachedCode != verifyCodeDto.Code)
                {
                    return BadRequest(new { isSuccess = false, message = "El código de verificación es incorrecto." });
                }

                /*En caso de que si encuentre el código y sea correcto con respecto al que entregó el usuario, 
                elimina el código del caché */
                _cache.Remove($"VerificationCode_{verifyCodeDto.UserId}");
                //Si el proceso fue exitoso retorna true y estado 200
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, message = "Código verificado exitosamente. Puedes proceder a restablecer tu contraseña." });
            }
            catch (Exception ex)
            {   
                //En caso de que se presente un error en el try, retorna false y estado 500
                Console.WriteLine($"Error general en RestorePasswordController (VerifyVerificationCode): {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { isSuccess = false, message = $"Ocurrió un error al verificar el código: {ex.Message}" });
            }
        }
    }
}