using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace NTP
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string ntpServer = "0.ch.pool.ntp.org";

            byte[] timeMessage = new byte[48];

            timeMessage[0] = 0x1B; //LI = 0 (no warning), VN = 3 (IPv4 only), Mode = 3 (Client Mode)

            IPEndPoint ntpReference = new IPEndPoint(Dns.GetHostAddresses(ntpServer)[0], 123);

            UdpClient client = new UdpClient();

            client.Connect(ntpReference);

            client.Send(timeMessage, timeMessage.Length);

            timeMessage = client.Receive(ref ntpReference);

            DateTime ntpTime = NtpPacket.ToDateTime(timeMessage);

            Console.WriteLine(ntpTime.ToLongDateString());

            Console.WriteLine(ntpTime.ToString("dd/MM/yyyy HH:mm:ss"));

            Console.WriteLine(ntpTime.ToString("dd/MM/yyyy"));

            Console.WriteLine(ntpTime.ToString("yyyy-MM-ddThh:mm:ssZ"));

            DateTime ntpTimeUtc = ntpTime;
            DateTime systemTimeUtc = DateTime.UtcNow;
            TimeSpan timeDiff = systemTimeUtc - ntpTimeUtc;
            Console.WriteLine($"Différence de temps : {timeDiff.TotalSeconds:F2} secondes");

            DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(ntpTimeUtc, TimeZoneInfo.Local);
            Console.WriteLine($"Heure locale : {localTime}");

            TimeZoneInfo swissTimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            DateTime swissTime = TimeZoneInfo.ConvertTimeFromUtc(ntpTimeUtc, swissTimeZone);
            Console.WriteLine($"Heure suisse : {swissTime}");

            TimeZoneInfo utcTimeZone = TimeZoneInfo.Utc;
            DateTime backToUtc = TimeZoneInfo.ConvertTime(localTime, TimeZoneInfo.Local, utcTimeZone);
            Console.WriteLine($"Retour vers UTC : {backToUtc}");

            DateTime i = DateTime.UtcNow;
            DisplayWorldClocks(i);

            client.Close();
        }

        static class NtpPacket
        {
            public static DateTime ToDateTime(byte[] ntpData)
            {
                ulong intPart = (ulong)ntpData[40] << 24 | (ulong)ntpData[41] << 16 | (ulong)ntpData[42] << 8 | (ulong)ntpData[43];
                ulong fractPart = (ulong)ntpData[44] << 24 | (ulong)ntpData[45] << 16 | (ulong)ntpData[46] << 8 | (ulong)ntpData[47];

                var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);
                var networkDateTime = (new DateTime(1900, 1, 1)).AddMilliseconds((long)milliseconds);

                return networkDateTime;
            }
        }

        

        public static void DisplayWorldClocks(DateTime utcTime)
        {
            var timeZones = new[]
            {
                ("UTC", TimeZoneInfo.Utc),
                ("New York", TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")),
                ("London", TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time")),
                ("Tokyo", TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time")),
                ("Sydney", TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time")),
            };
            foreach (var(name, tz)in timeZones)
            {
                var localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, tz);
                Console.WriteLine($"{name}: {localTime:yyyy-MM-dd HH:mm:ss}");
            }
        }
    }
}
