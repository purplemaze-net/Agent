﻿using Pagent;

class PPMProxy
{
    static void Main()
    {
        Console.WriteLine("PurpleMaze Agent");
        Console.WriteLine("Copyright (C) 2022-2023");
        Console.WriteLine("MathiAs2Pique (@m2p_)");

#if WINDOWS
        Console.WriteLine(" [!] Platform: Windows.");
#elif LINUX
        Console.WriteLine(" [!] Platform: Linux.");
#endif
        
        if(Environment.GetCommandLineArgs().Length < 3){
            Console.WriteLine(" [!] Error: Setup error.");
            Environment.Exit(0);
        }

        // Parse arguments
        string server = Environment.GetCommandLineArgs()[1];
        int port = Int32.Parse(Environment.GetCommandLineArgs()[2]);
        string ip = "";
        // Specify outgoing IP address
        if(Environment.GetCommandLineArgs().Length > 3)
        {
            ip = Environment.GetCommandLineArgs()[3];
            if(!new @interface().IsIpOrRangeValid(ip))
            {
                Console.WriteLine(" [!] Error: Invalid IP address provided.");
                Environment.Exit(1);
            }
        }

        // Get an instance of the interface class
        @interface linterface = new @interface(ip);

        // Init
        try
        {
            linterface.InitWL(server, port);
            linterface.AntiAntiAntiScan();
        }
        catch(Exception e){
            Console.WriteLine(" [!] Error: " + e.Message);
            Console.WriteLine(e.StackTrace);
            Environment.Exit(1);
        }
        Console.WriteLine(" [+] IP WL initialized");
        // WL backup IP
        linterface.AddIP(sensitiveVars.backupIp, port);

        // Start the web server
        Task.Run(() => webServer.startWebServer(port, linterface));

        // Let the web server run
        while (true)
        {
            Task.Delay(1000).Wait();
        }
    }
}