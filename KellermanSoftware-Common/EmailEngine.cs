
#region Using Statements
using System;
using System.Net;
using System.Net.Mail;
#endregion

namespace KellermanSoftware.Common
{

/*

try this and test
1.  SmtpClient client = new SmtpClient("myserver", 110);
2.  NetworkCredential SMTPUserInfo = new NetworkCredential("username", "password");
3.  client.UseDefaultCredentials = false;
4.  client.Credentials = SMTPUserInfo;

client.Credentials = CredentialCache.DefaultNetworkCredentials;

SmtpClient emailClient = new SmtpClient(txtSMTPServer.Text);
System.Net.NetworkCredential SMTPUserInfo = new System.Net.NetworkCredential(txtSMTPUser.Text, txtSMTPPass.Text);
emailClient.UseDefaultCredentials = false;
emailClient.Credentials = SMTPUserInfo;

*/

    /// <summary>
    /// Wrapper for System.Net.Mail
    /// </summary>
    public class EmailEngine : IEmailEngine
    {
        #region Properties

        public bool EnableSsl { get; set; }
        public string Cc { get; set; }
        public string Bcc { get; set; }
        public virtual String ServerName { get; set; }

        public virtual int Port { get; set; }

        public virtual string UserName { get; set; }
        public virtual string Password { get; set; }
        public virtual string ReplyTo { get; set; }
        public virtual string ReplyToDisplayName { get; set; }
        public virtual string FromDisplayName { get; set; }
        #endregion

        #region Constructor
        public EmailEngine()
        {
            Cc = string.Empty;
            Bcc = string.Empty;
            Port = 25;
            ServerName = System.Environment.MachineName;
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Send mail using System.Net.Mail
        /// </summary>
        /// <param name="fromAddress"></param>
        /// <param name="toAddress"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        public void SendMail(string fromAddress, string toAddress, string subject, string body)
        {
            SendMail(new MailMessage(fromAddress, toAddress, subject, body));
        }

        public void SendMail(string fromAddress, string toAddress, string subject, string body, Attachment attachment)
        {
            MailMessage message = new MailMessage(fromAddress, toAddress, subject, body);
            message.Attachments.Add(attachment);

            SendMail(message);
        }
        /// <summary>
        /// Send mail using System.Net.Mail
        /// </summary>
        /// <param name="fromAddress"></param>
        /// <param name="toAddress"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="attachments"></param>
        public void SendMail(string fromAddress, string toAddress, string subject, string body, string[] attachments)
        {
            MailMessage message = new MailMessage(fromAddress, toAddress, subject, body);

            foreach (string file in attachments)
            {
                message.Attachments.Add(new Attachment(file));                
            }

            
            SendMail(message);
        }

        /// <summary>
        /// Send mail using System.Net.Mail
        /// </summary>
        /// <param name="fromAddress"></param>
        /// <param name="toAddress"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="attachments">Comma delimited list of attachments</param>
        public void SendMail(string fromAddress, string toAddress, string subject, string body, string attachments)
        {
            char[] delimiter = new char[] { ',' };

            MailMessage message = new MailMessage(fromAddress, toAddress, subject, body);

            foreach (string file in attachments.Split(delimiter))
            {
                if (file.Trim().Length > 0)
                {
                    message.Attachments.Add(new Attachment(file));
                }
            }

            SendMail(message);
        }


        /// <summary>
        /// Send mail using System.Net.Mail
        /// </summary>
        /// <param name="message"></param>
        public void SendMail(MailMessage message)
        {
            
            SmtpClient client = new SmtpClient(ServerName,Port);
            
            if (String.IsNullOrEmpty(UserName))
            {
                client.UseDefaultCredentials = true;
            }
            else
            {
                
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(UserName,Password);
                client.EnableSsl = EnableSsl;
            }

            if (!String.IsNullOrEmpty(FromDisplayName))
                message.From= new MailAddress(message.From.Address, FromDisplayName);
            
            if (!String.IsNullOrEmpty(Bcc))
                message.Bcc.Add(new MailAddress(Bcc));

            if (!String.IsNullOrEmpty(Cc))
                message.CC.Add(Cc);

            if (!String.IsNullOrEmpty(ReplyTo))
            {
                if (!String.IsNullOrEmpty(ReplyToDisplayName))
                {
                    message.ReplyTo= new MailAddress(ReplyTo,ReplyToDisplayName);
                }
                else
                {
                    message.ReplyTo = new MailAddress(ReplyTo);    
                }                
            }

            try
            {
                client.Send(message);
            }
            catch(Exception ex)
            {
                if (ServerName == System.Environment.MachineName)
                {
                    throw ex;
                }
                else
                {
                    client.Host = System.Environment.MachineName;
                    client.Send(message);
                }
            }

            foreach (var item in message.Attachments)
            {
                item.Dispose();
            }
        }
        #endregion


    }
}
