using LabOOP.Models;
using Microsoft.AspNet.Identity;
using MimeKit;
using System.Net.Mail;

namespace LabOOP.IdentityClass
{
    public class EmailService : IIdentityMessageService
    {
        private readonly EmailConfiguration emailConfiguration;
        public EmailService(EmailConfiguration emailConfiguration)
        {
            this.emailConfiguration = emailConfiguration;
        }
        public void CreateMessage(string email, string body, string subject)
        {
            IdentityMessage mes = new IdentityMessage { Destination = email, Body = body, Subject = subject };
            SendAsync(mes).Wait();  
        }
        public Task SendAsync(IdentityMessage message)
        {
            SmtpClient smtpClient = new SmtpClient(emailConfiguration.SmtpServer, emailConfiguration.Port);
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new System.Net.NetworkCredential(emailConfiguration.Username, emailConfiguration.Password);
            smtpClient.EnableSsl = true;
            var mail = CreateMail(message.Destination, message.Body, message.Subject);
            mail.IsBodyHtml = true;
            return smtpClient.SendMailAsync(mail);
        }
        private MailMessage CreateMail(string destination, string body, string subject)
        {
             var mail = new MailMessage(emailConfiguration.From, destination);
             mail.Body = body;
             mail.Subject = subject;
             return mail;
        }
    }
}
