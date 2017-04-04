//   Emailer.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2017 Allis Tauri

using System.Net;
using System.Net.Mail;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

namespace AT_Utils
{
//    #if DEBUG
//    [KSPAddon(KSPAddon.Startup.Instantly, false)]
//    public class EmailerTester : MonoBehaviour
//    {
//        void Awake()
//        {
//            Emailer.SendLocalSelf("allista@gmail.com", "KSP: Test message", "Test message");
//        }
//    }
//    #endif

    public static class Emailer
    {
        static MailMessage MakeMalil(string from_adr, string to_adr,
                                     string sbj, string msg, params object[] args)
        {
            var mail = new MailMessage();

            mail.From = new MailAddress(from_adr);
            mail.To.Add(to_adr);
            mail.Subject = sbj;
            mail.Body = Utils.Format(msg, args);
            return mail;
        }

        static void SendMail(MailMessage mail, SmtpClient smtp)
        {
            try { smtp.Send(mail); }
            catch(SmtpException e)
            {
                Utils.Log("Unable to send e-mail from: {}, to: {}\n{}", mail.From, mail.To, e.ToString());
            }
        }

        public static void SendGmailSelf(string from_adr, string password, 
                                         string sbj, string msg, params object[] args)
        { SendGmail(from_adr, from_adr, password, sbj, msg, args); }

        public static void SendGmail(string from_adr, string to_adr, string password, 
                                     string sbj, string msg, params object[] args)
        {
            
            var mail = MakeMalil(from_adr, to_adr, sbj, msg, args);
            var smtp = new SmtpClient("smtp.gmail.com", 587);
            smtp.Timeout = 2000;
            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential(from_adr, password) as ICredentialsByHost;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            SendMail(mail, smtp);
        }

        public static void SendLocalSelf(string to_adr, string sbj, string msg, params object[] args)
        { SendLocal(to_adr, to_adr, sbj, msg, args); }

        public static void SendLocal(string from_adr, string to_adr, string sbj, string msg, params object[] args)
        {
            var mail = MakeMalil(from_adr, to_adr, sbj, msg, args);
            var smtp = new SmtpClient("localhost");
            SendMail(mail, smtp);
        }
    }
}

