using System.Collections.Generic;
using System.Net.Mail;
using System.Text;

namespace KellermanSoftware.Common
{
    /// <summary>
    /// Custom mock for the email engine
    /// </summary>
    public class EmailEngineMock : IEmailEngine
    {
        public bool EnableSsl { get; set; }
        public string Cc { get; set; }
        public string Bcc { get; set; }
        public string ServerName { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ReplyTo { get; set; }
        public string ReplyToDisplayName { get; set; }
        public string FromDisplayName { get; set; }

        public List<string> Emails { get; set; }

        public EmailEngineMock()
        {
            Emails = new List<string>();
        }


        public void SendMail(string fromAddress, string toAddress, string subject, string body)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("From:  " + fromAddress);
            stringBuilder.AppendLine("To:  " + toAddress);
            stringBuilder.AppendLine("Subject:  " + subject);
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(body);

            Emails.Add(stringBuilder.ToString());
        }

        public void SendMail(string fromAddress, string toAddress, string subject, string body, Attachment attachment)
        {
            SendMail(fromAddress,toAddress,subject,body);
        }

        public void SendMail(string fromAddress, string toAddress, string subject, string body, string[] attachments)
        {
            SendMail(fromAddress, toAddress, subject, body);
        }

        public void SendMail(string fromAddress, string toAddress, string subject, string body, string attachments)
        {
            SendMail(fromAddress, toAddress, subject, body);
        }

        public void SendMail(MailMessage message)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("From:  " + message.From);
            stringBuilder.AppendLine("To:  " + message.To);
            stringBuilder.AppendLine("Subject:  " + message.Subject);
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(message.Body);

            Emails.Add(stringBuilder.ToString());
        }
    }
}
