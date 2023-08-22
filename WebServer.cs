﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Pagent
{
    public class webServer
    {
        public static HttpListener listener;
        public static string key;

        public webServer(){
            key = sensitiveVars.reqKey;
        }

        public static long getTimestamp()
        {
            return DateTimeOffset.Now.ToUnixTimeSeconds();
        }

        public static async Task HandleIncomingConnections(int port, @interface linterface)
        {
            bool run = true;
            while (run)
            {
                try
                {
                    HttpListenerContext ctx = await listener.GetContextAsync();
                    HttpListenerRequest req = ctx.Request;
                    HttpListenerResponse resp = ctx.Response;

                    string returnStr = "{\"success\":false, \"error\":\"Generic Error.\"}";
                    bool doContinue = !((!req.Headers.AllKeys.Contains("Key") || req.Headers["Key"] != key) && (!req.Headers.AllKeys.Contains("key") || req.Headers["key"] != key));

                    if (!doContinue)
                    {                        
                        Console.WriteLine("[!] Wrong / missing authorization");
                    }
                    
                    if (req.RemoteEndPoint != null && !sensitiveVars.queryIPs.Contains(req.RemoteEndPoint.Address.ToString()))
                    {
                        doContinue = false;
                        Console.WriteLine("[!] Wrong query IP: " + req.RemoteEndPoint.Address.ToString());
                    }

                    // Check the path
                    if (doContinue && req.Url.AbsolutePath.Contains("/alive"))
                    {
                        Console.WriteLine("[.] Alive request received.");
                        returnStr = "{\"success\":true}";
                    }
                    else if (doContinue && req.Url.AbsolutePath.Contains("/wl"))
                    {
                        Console.WriteLine($"[.] Whitelist request received. [Method:{req.HttpMethod}  IP: {req.RemoteEndPoint}]");
                        // check method
                        if (req.HttpMethod == "POST")
                        {
                            // get POST data
                            string? ip = req.QueryString["ip"];

                            if (ip != null)
                            {
                                linterface.AddIP(ip, port);
                                returnStr = "{\"success\":true}";
                            }
                            else
                                doContinue = false;
                        }
                        else if (req.HttpMethod == "DELETE")
                        {
                            // get POST data
                            string? ip = req.QueryString["ip"];
                            
                            if(ip != null)
                            {
                                linterface.RemoveIP(ip, port);
                                returnStr = "{\"success\":true}";
                            }
                            else
                                doContinue = false;
                        }
                        else
                        {
                            doContinue = false;
                        }
                    }
                    else
                    {
                        doContinue = false;
                    }

                    if (!doContinue)
                    {
                        resp.StatusCode = 400;
                        resp.ContentType = "application/json";
                        byte[] buffer = Encoding.UTF8.GetBytes(returnStr);
                        resp.ContentLength64 = buffer.Length;
                        await resp.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                        resp.Close();
                    }
                    else
                    {
                        resp.StatusCode = 200;
                        resp.ContentType = "application/json";
                        byte[] buffer = Encoding.UTF8.GetBytes(returnStr);
                        resp.ContentLength64 = buffer.Length;
                        await resp.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                        resp.Close();
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        public static Task startWebServer(int port, @interface linterface)
        {

            string url = $"http://*:{sensitiveVars.bindPort}/";
            listener = new HttpListener();
            listener.Prefixes.Add(url);

            while (true)
            {
                try
                {
                    Console.WriteLine(" [\\] Starting web server...");
                    listener.Start();
                    Console.WriteLine(" [\\] Web server started.");
                    Console.WriteLine(" [\\] Listening.");
                    Task listenTask = HandleIncomingConnections(port, linterface);
                    listenTask.GetAwaiter().GetResult();
                }
                catch (Exception e)
                {
                    Console.WriteLine("[!] Error:\n" + e.Message);
                    Console.WriteLine("[!] Exiting...");
                    Environment.Exit(1);
                }
                finally
                {
                    if (listener.IsListening)
                        listener.Stop();
                }
                listener.Close();
            }
        }
    }
}
