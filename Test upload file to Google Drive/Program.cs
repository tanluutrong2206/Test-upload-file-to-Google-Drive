
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Template_certificate
{
    class Program
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/drive-dotnet-quickstart.json
        // C:\Users\tanlu\Documents\.credentials\drive-dotnet-quickstart.json       this is my folder path
        //Reading https://developers.google.com/drive/v3/web/quickstart/dotnet to get more information
        static string ApplicationName = "Funix's Certificate Generation Automatical";

        static void Main(string[] args)
        {
            Connect connect = new Connect();

            UserCredential credential = connect.GetAuthenication();

            // Create Drive API service.
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            //set timeout
            service.HttpClient.Timeout = TimeSpan.FromMinutes(10);

            //get list of all folders
            var files = new List<Google.Apis.Drive.v3.Data.File>();
            //files = connect.RetrieveAllPdfFileDirectoryFolders(service, "1ebXLfc1SRpsp0uPZZ33pn9IJLCHbAYqa");

            //display to console
            foreach (var file in files)
            {
                Console.WriteLine("{0} ({1})", file.Name, file.Id);
                //if (file.Name.Contains("MindMeister"))
                //{
                //    string fileName = "SE00220x-FUN180037-Vũ Anh Tuấn.pdf";
                //    //CreateNewFolder(file.Id, service, "hello world");
                //    connect.CreateNewFile(file.Id, service, $"../../files/{fileName}", fileName);
                //}
            }
            Console.Read();
        }
    }
}