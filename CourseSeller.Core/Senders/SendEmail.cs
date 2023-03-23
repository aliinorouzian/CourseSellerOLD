using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace CourseSeller.Core.Senders
{
    public interface ISendEmail
    {
        public Task<bool> Send(string to, string subject, string body);
    }

    public class SendEmail : ISendEmail
    {
        private IConfiguration _conf;

        public SendEmail(IConfiguration conf)
        {
            _conf = conf;
        }

        public async Task<bool> Send(string to, string subject, string body)
        {
            var GmailSettings = _conf.GetSection("Emails").GetSection("Gmail");

            MailMessage mail = new MailMessage();

            SmtpClient SmtpServer = new SmtpClient(GmailSettings.GetSection("HostName").Value,
                Convert.ToInt32(GmailSettings.GetSection("Port").Value));
            SmtpServer.EnableSsl = true;

            mail.From = new MailAddress(GmailSettings.GetSection("EmailAddress").Value, _conf["SiteName"]);
            mail.To.Add(to);
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;


            //System.Net.Mail.Attachment attachment;
            // attachment = new System.Net.Mail.Attachment("c:/textfile.txt");
            // mail.Attachments.Add(attachment);

            SmtpServer.Credentials = new System.Net.NetworkCredential(GmailSettings.GetSection("EmailAddress").Value,
                GmailSettings.GetSection("Password").Value);

            try
            {
                await SmtpServer.SendMailAsync(mail);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }

        }
    }
}