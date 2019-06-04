namespace WatchMyPrices.Notification
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Mail;
    using System.Text;
    using System.Threading.Tasks;

    public class EmailNotification : INotification
    {
        public void Notify()
        {
            using (SmtpClient smtp = new SmtpClient())
            {
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.UseDefaultCredentials = false;
                smtp.EnableSsl = true;
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.Credentials = new NetworkCredential("seeanewprice@gmail.com", "6bW774oIvE");
                smtp.Send("seeanewprice@gmail.com", "jesse.prieur@gmail.com", "Seen A New Price!", "Lorem Ipsum seen a new price!");
            }
        }
    }
}
