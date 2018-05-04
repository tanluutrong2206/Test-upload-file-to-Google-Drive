
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

namespace Test_upload_file_to_Google_Drive
{
    class Program
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/drive-dotnet-quickstart.json
        // C:\Users\tanlu\Documents\.credentials\drive-dotnet-quickstart.json       this is my folder path
        //Reading https://developers.google.com/drive/v3/web/quickstart/dotnet to get more information

        static string[] Scopes = { Google.Apis.Drive.v3.DriveService.Scope.Drive };
        static string ApplicationName = "Drive API .NET Quickstart";

        static void Main(string[] args)
        {
            UserCredential credential;

            using (var stream =
                new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/drive-dotnet-quickstart.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Drive API service.
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            //set timeout
            service.HttpClient.Timeout = TimeSpan.FromMinutes(10);

            //get list of all folders
            var files = retrieveAllFolders(service);

            //display to console
            foreach (var file in files)
            {
                Console.WriteLine("{0} ({1})", file.Name, file.Id);
                if (file.Name.Contains("MindMeister"))
                {
                    //CreateNewFolder(file.Id, service, "hello world");
                    CreateNewFile(file.Id, service, "../../files/CC00706x-FUN180029-Kiều Hoàng Long.pdf", "CC00706x-FUN180029-Kiều Hoàng Long.pdf");
                }
            }
            Console.Read();
        }

        public static List<Google.Apis.Drive.v3.Data.File> retrieveAllFolders(DriveService service)
        {
            List<Google.Apis.Drive.v3.Data.File> result = new List<Google.Apis.Drive.v3.Data.File>();
            Google.Apis.Drive.v3.FilesResource.ListRequest request = service.Files.List();

            //setup filter only select folder
            // cant use finding extension because in google drive docs file, powerpoint, excel, ... does not show extension
            request.Q = "mimeType = 'application/vnd.google-apps.folder'";

            do
            {
                try
                {
                    FileList files = request.Execute();
                    result.AddRange(files.Files);

                    request.PageToken = files.NextPageToken;
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occurred: " + e.Message);
                    request.PageToken = null;
                }
            } while (!String.IsNullOrEmpty(request.PageToken));

            return result;
        }

        internal static void CreateNewFolder(string folderID, DriveService driveService, string folderName)
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder",
                Parents = new List<string> { folderID },
                //Size = 28000,
            };
            FilesResource.CreateRequest request;

            request = driveService.Files.Create(fileMetadata);
            Google.Apis.Drive.v3.Data.File file = request.Execute();


            Console.WriteLine("File ID: " + file.Id);
        }

        internal static void CreateNewFile(string folderID, DriveService driveService, string filePath, string fileName)
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = fileName,

                //find Mimetype corresponding with type of file inside below link
                //https://developers.google.com/drive/v3/web/integrate-open#open_and_convert_google_docs_in_your_app
                MimeType = "application/pdf",

                //Choose what parent folder containt file
                //Earch file/folder inside google drive has identify id
                //Take that when reading all folder when start or after create new folder
                Parents = new List<string> { folderID },
            };

            FilesResource.CreateMediaUpload request;

            using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Open))
            {
                request = driveService.Files.Create(fileMetadata, stream, "application/pdf");
                request.Fields = "id";
                request.Upload();
            }

            //After create folder, get back folder to take folderID
            var file = request.ResponseBody;

            Console.WriteLine("File ID: " + file.Id);
        }
    }
}