using MimeKit;
using Microsoft.AspNetCore.Identity.UI.Services;
using CrossPlanner.Repository.Wrapper;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace CrossPlanner.Service.Messages
{
    public class EmailSender : IEmailSender
    {
        private readonly IRepositoryWrapper _repositoryWrapper;

        public EmailSender(IRepositoryWrapper repositoryWrapper)
        {
            _repositoryWrapper = repositoryWrapper;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var mailServer = _repositoryWrapper.MailServerRepository
                .FindByCondition(x => x.Active)
                .FirstOrDefault();

            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Cross Planner", mailServer.MailServerUserName));
                message.To.Add(new MailboxAddress("", email));
                message.Subject = subject;

                // Add plain text alternative
                var builder = new BodyBuilder();
                builder.HtmlBody = htmlMessage;
                builder.TextBody = HtmlToPlainText(htmlMessage);

                message.Body = builder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(mailServer.MailServerIP, mailServer.MailServerPort, SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(mailServer.MailServerUserName, mailServer.MailServerPassword);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string HtmlToPlainText(string html)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            return doc.DocumentNode.InnerText;
        }
    }
}