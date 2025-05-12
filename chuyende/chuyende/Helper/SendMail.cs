using System;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Web;

namespace chuyende.Helper
{
    public class SendMail
    {
        public bool SendMailFunction(string to, string subject, string bodyHtml, HttpPostedFileBase file = null)
        {
            string hostEmail = "smtp.gmail.com";
            int portEmail = 587;
            string emailSender = "storeelectronics457@gmail.com";
            string passwordSender = "bity jzxp zpbq tvep";

            try
            {
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(emailSender);
                mail.To.Add(to);
                mail.Subject = subject;
                mail.IsBodyHtml = true;

                if (file != null && file.ContentLength > 0)
                {
                    // Tạo content ID duy nhất
                    string contentId = Guid.NewGuid().ToString();

                    // Tạo attachment từ file upload
                    Attachment inline = new Attachment(file.InputStream, file.FileName);
                    inline.ContentId = contentId;
                    inline.ContentDisposition.Inline = true;
                    inline.ContentDisposition.DispositionType = DispositionTypeNames.Inline;

                    mail.Attachments.Add(inline);

                    // Chèn ảnh vào nội dung HTML
                    bodyHtml += $"<br><img src=\"cid:{contentId}\" style=\"max-width:100%;\" />";
                }

                mail.Body = bodyHtml;

                SmtpClient smtp = new SmtpClient(hostEmail, portEmail);
                smtp.Credentials = new NetworkCredential(emailSender, passwordSender);
                smtp.EnableSsl = true;
                smtp.Send(mail);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Lỗi gửi mail: " + ex.Message);
                return false;
            }
        }
    }
}
