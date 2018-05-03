
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
        // C:\Users\tanlu\Documents\.credentials\drive-dotnet-quickstart.json
        static string[] Scopes = { Google.Apis.Drive.v2.DriveService.Scope.DriveReadonly };
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
            


            //get list of all folders
            var files = retrieveAllFolders(service);

            //display to console
            foreach (var file in files)
            {
                Console.WriteLine("{0} ({1})", file.Name, file.Id);
                if (file.Name.Contains("MindMeister"))
                {
                    UploadFileToGoogleDrive(file.Id, service);
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

        internal static void UploadFileToGoogleDrive(string folderID, DriveService driveService)
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = "photo.jpg",
                //MimeType = "application/vnd.google-apps.document",
                Parents = new List<string>
                {
                    folderID
                }
            };
            FilesResource.CreateMediaUpload request;
            using (var stream = new FileStream("C:\\Users\\tanlu\\Downloads\\safe_image.jpg", FileMode.Open))
            {
                request = driveService.Files.Create(
                    fileMetadata, stream, "image/jpeg");
                request.Fields = "id";
                request.Upload();
            }
            var file = request.ResponseBody;
            Console.WriteLine("File Uploaded");
            //Console.WriteLine("File ID: " + file.Id);
            //mind: 0B3Cf6r5C_7CfUFJtZjZRZEZSdnM
        }
    }
}