using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Mail;
using WhatsappNet.Api.Models.WhatsappCloud;
using WhatsappNet.Api.Services.WhatsappCloud.SendMessage;
using WhatsappNet.Api.Util;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Whatsapp.Net.Controllers
{
    [ApiController]
    [Route("api/whatsapp")]
    public class WhatsappController : Controller
    {
        private readonly IWhatsappCloudSendMessage _whatsappCloudSendMessage;
        private readonly IUtil _util;
      
        public WhatsappController(IWhatsappCloudSendMessage whatsappCloudSendMessage, IUtil util)
        {
            _whatsappCloudSendMessage = whatsappCloudSendMessage;
            _util = util;
        }

        [HttpGet("test")]
        public async Task<IActionResult> Sample()
        {
            var data = new 
            {
                messaging_product = "whatsapp",
                to = "Numero de telefono",
                type = "text",
                text = new
                {
                    body = "Hola Mundo"
                }
            };         

            var result = await _whatsappCloudSendMessage.Execute(data);

            return Ok("ok sample");

        }

        [HttpGet]
        public IActionResult VerifyToken()
        {
            string AccessToken = "ACCESS_TOKEN";

            var token = Request.Query["hub.verify_token"].ToString();
            var challenge = Request.Query["hub.challenge"].ToString();

            if (challenge != null && token != null && token == AccessToken)
            {
                return Ok(challenge);
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPost]
        public async Task<IActionResult> ReceivedMessage([FromBody] WhatsAppCloudModel body)
        {
            try
            {
                var Message = body.Entry[0]?.Changes[0]?.Value?.Messages[0];
                if (Message != null)
                {
                    var userNumber = Message.From;
                    var userText = GetUserText(Message);

                    object objectMessage;

                    if (userText.ToUpper().Contains("HOLA"))
                    {
                        objectMessage = _util.TextMessage("Hola, soy un bot de prueba, ¿en que te puedo ayudar?", userNumber);
                    }
                    else if (userText.ToUpper().Contains("GRACIAS") || userText.ToUpper().Contains("AGRADECIDO"))
                    {
                        objectMessage = _util.TextMessage("De nada, estoy para ayudarte", userNumber);
                    }
                    else if (userText.ToUpper().Contains("Ubicación", userNumber) 
                    {
                        objectMessage = _util.TextMessage("Estamos ubicados en la avenida siempre viva 123", userNumber);
                    }
                    else
                    {
                        objectMessage = _util.TextMessage("No entiendo lo que me dices, ¿puedes ser mas especifico?", userNumber);
                    }
                     
                    await _whatsappCloudSendMessage.Execute(objectMessage);
                }

                return Ok("EVENT_RECEIVED");
            }
            catch (Exception ex)
            {
                return Ok("EVENT_RECEIVED");
            }
        }

        private string GetUserText(Message message)
        {
            string TypeMessage = message.Type;

            if (TypeMessage.ToUpper() == "TEXT")
            {
                return message.Text.Body;
            }
            else if (TypeMessage.ToUpper() == "INTERACTIVE")
            {
                string interactiveType = message.Interactive.Type;

                if (interactiveType.ToUpper() == "LIST_REPLY")
                {
                    return message.Interactive.List_Reply.Title;
                }
                else if (interactiveType.ToUpper() == "BUTTON_REPLY")
                {
                    return message.Interactive.Button_Reply.Title;
                }
                else
                {
                    return string.Empty;
                }

            }
            else
            {
                return string.Empty;
            }
        }
    }

}

