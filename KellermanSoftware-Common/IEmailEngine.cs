using System.Net.Mail;

namespace KellermanSoftware.Common
{
    public interface IEmailEngine
    {
        bool EnableSsl { get; set; }
        string Cc { get; set; }
        string Bcc { get; set; }
        string ServerName { get; set; }
        int Port { get; set; }
        string UserName { get; set; }
        string Password { get; set; }
        string ReplyTo { get; set; }
        string ReplyToDisplayName { get; set; }
        string FromDisplayName { get; set; }

        void SendMail(string fromAddress, string toAddress, string subject, string body);
        void SendMail(string fromAddress, string toAddress, string subject, string body, Attachment attachment);
        void SendMail(string fromAddress, string toAddress, string subject, string body, string[] attachments);
        void SendMail(string fromAddress, string toAddress, string subject, string body, string attachments);
        void SendMail(MailMessage message);
    }
}
