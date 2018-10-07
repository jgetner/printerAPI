using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server;
using System.Drawing.Printing;
using System.Management;
using System.IO;
using System.Runtime.InteropServices;

namespace printerAPI
{
    class Program
    {
        

        static void Main(string[] args)
        {

            PrintManager printManager = new PrintManager();


            //configure the port the server should listen on
            ServerConfig config = new ServerConfig();
            config.port = 8080;

            //configure the routes the server should listen for
            ServerRoute route = new ServerRoute();
            route.method = ServerRouteMethod.GET;
            route.route = "/";
            route.action = (ServerRequest request, ServerResponse response) => {
                response.Status(200, "Welcome to THE SBI Printing Controller");
                return response;
            };

            ServerRoute printerListRoute = new ServerRoute();
            printerListRoute.method = ServerRouteMethod.GET;
            printerListRoute.route = "/list";
            printerListRoute.action = (ServerRequest request, ServerResponse response) => {

                Console.WriteLine("Getting the list of installed printers");

                string json = "{";

                foreach (string printer in printManager.installed)
                {
                    json += "\"" + printer + "\",";
                    Console.WriteLine("This is an installed printer option " + printer);
                }

                json += "}";

                response.Json(json);
                return response;

            };

            ServerRoute getDefaultPrinterRoute = new ServerRoute();
            getDefaultPrinterRoute.method = ServerRouteMethod.GET;
            getDefaultPrinterRoute.route = "/get/default";
            getDefaultPrinterRoute.action = (ServerRequest request, ServerResponse response) => {
                Console.WriteLine("Getting the default printer");
                string json = "{";

                json += "\"" + printManager.getDefaultPrinter() + "\"";

                json += "}";

                response.Json(json);
                return response;
            };

            ServerRoute setDefaultPrinterRoute = new ServerRoute();
            setDefaultPrinterRoute.method = ServerRouteMethod.GET;
            setDefaultPrinterRoute.route = "/set/default";
            setDefaultPrinterRoute.action = (ServerRequest request, ServerResponse response) => {
                string[] name = request.request.QueryString.GetValues("printer");
               

                foreach (string printer in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
                {
                    if (name[0] == printer)
                    {
                        Console.WriteLine("setting default printer to:" + name[0]);
                        printManager.setDefaultPrinter(name[0]);
                        response.Status(200, "setting default printer to:" + name[0]);
                        return response;
                        
                    }
                }


                response.Status(400, "Bad Request");
                return response;

            };

            ServerRoute printDefaultRoute = new ServerRoute();
            printDefaultRoute.method = ServerRouteMethod.POST;
            printDefaultRoute.route = "/print";
            printDefaultRoute.action = (ServerRequest request, ServerResponse response) => {

                PrintDocument document = new PrintDocument();
                
                if(document.PrinterSettings.PrinterName == printManager.getDefaultPrinter())
                {
                    response.Status(200, "PRINTING");
                    return response;
                }

                response.Status(400, "The Printer has been changed");
                return response;
            };

            //add the route to the routes
            ServerRoutes routes = new ServerRoutes();
            routes.Add(route);
            routes.Add(printerListRoute);
            routes.Add(getDefaultPrinterRoute);
            routes.Add(setDefaultPrinterRoute);

            try
            {
                ServerListener listener = new ServerListener(config, routes);

                listener.Start();
                Console.ReadKey();
                listener.Stop();
            }

            catch(Exception e)
            {
                Console.WriteLine("Error:" + e.Message);
               
            }

            return;

        }


       
    }
}
