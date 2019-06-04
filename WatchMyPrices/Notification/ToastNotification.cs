namespace WatchMyPrices.Notification
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Windows.Data.Xml.Dom;
    using Windows.UI.Notifications;

    public class ToastNotification : INotification
    {
        public void Notify()
        {
            string xml = $@"
                <toast>
                    <visual>
                        <binding template='ToastGeneric'>
                            <text>Some title</text>
                            <text>Lorem ipsum dolor sit amet, consectetur adipiscing elit.</text>
                        </binding>
                    </visual>
                </toast>";

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);
            var toast = new Windows.UI.Notifications.ToastNotification(xmlDoc);
            ToastNotificationManager.CreateToastNotifier("WatchMyPrices").Show(toast);
        }
    }
}
