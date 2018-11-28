using System.Net.Mail;

namespace KellermanSoftware.Common
{
    /// <summary>
    /// Interface to use for mocking the Email Engine
    /// </summary>
    public interface IEmailEngine
    {
        /// <summary>
        /// If true, SSL is enabled
        /// </summary>
        bool EnableSsl { get; set; }

        /// <summary>
        /// Carbon Copy
        /// </summary>
        string Cc { get; set; }

        /// <summary>
        /// Blind Carbon Copy
        /// </summary>
        string Bcc { get; set; }

        /// <summary>
        /// Name of the server
        /// </summary>
        string ServerName { get; set; }

        /// <summary>
        /// Port of the server
        /// </summary>
        int Port { get; set; }

        /// <summary>
        /// User name to use when authenticating
        /// </summary>
        string UserName { get; set; }

        /// <summary>
        /// Password to use when authenticating
        /// </summary>
        string Password { get; set; }

        /// <summary>
        /// The Reply to email address
        /// </summary>
        string ReplyTo { get; set; }

        /// <summary>
        /// The Reply to display name
        /// </summary>
        string ReplyToDisplayName { get; set; }

        /// <summary>
        /// The From Display name
        /// </summary>
        string FromDisplayName { get; set; }

        /// <summary>
        /// Send mail
        /// </summary>
        /// <param name="fromAddress"></param>
        /// <param name="toAddress"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        void SendMail(string fromAddress, string toAddress, string subject, string body);

        /// <summary>
        /// Send mail with an attachment
        /// </summary>
        /// <param name="fromAddress"></param>
        /// <param name="toAddress"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="attachment"></param>
        void SendMail(string fromAddress, string toAddress, string subject, string body, Attachment attachment);

        /// <summary>
        /// Send mail with multiple attachments
        /// </summary>
        /// <param name="fromAddress"></param>
        /// <param name="toAddress"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="attachments"></param>
        void SendMail(string fromAddress, string toAddress, string subject, string body, string[] attachments);

        /// <summary>
        /// Send mail with an attachment
        /// </summary>
        /// <param name="fromAddress"></param>
        /// <param name="toAddress"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="attachments"></param>
        void SendMail(string fromAddress, string toAddress, string subject, string body, string attachments);

        /// <summary>
        /// Send mail using a MailMessage object
        /// </summary>
        /// <param name="message"></param>
        void SendMail(MailMessage message);
    }
}
