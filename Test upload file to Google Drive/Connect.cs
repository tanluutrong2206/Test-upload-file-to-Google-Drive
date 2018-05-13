using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Template_certificate
{
    public class Connect
    {
        private string[] Scopes = { DriveService.Scope.Drive };
        public string FileId { get; set; }

        public UserCredential GetAuthenication()
        {
            UserCredential credential;

            using (var stream =
                new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = Environment.GetFolderPath(
                    Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/drive-dotnet-quickstart.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }

            return credential;
        }

        public List<Google.Apis.Drive.v3.Data.File> RetrieveAllFolders(DriveService service, string parentFolderId)
        {
            List<Google.Apis.Drive.v3.Data.File> result = new List<Google.Apis.Drive.v3.Data.File>();
            FilesResource.ListRequest request = service.Files.List();

            //setup filter only select folder
            // cant use finding extension because in google drive docs file, powerpoint, excel, ... does not show extension
            string filter = "mimeType = 'application/vnd.google-apps.folder' and trashed = false";
            if (!string.IsNullOrEmpty(parentFolderId))
            {
                filter += $" and '{parentFolderId}' in parents";
            }
            request.Q = filter;
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

        public List<Google.Apis.Drive.v3.Data.File> RetrieveAllPdfFileDirectoryFolders(DriveService service, string parentId)
        {
            List<Google.Apis.Drive.v3.Data.File> result = new List<Google.Apis.Drive.v3.Data.File>();
            FilesResource.ListRequest request = service.Files.List();

            //setup filter only select folder
            // cant use finding extension because in google drive docs file, powerpoint, excel, ... does not show extension
            string filter = "mimeType = 'application/pdf' and trashed = false ";

            if (!string.IsNullOrEmpty(parentId))
            {
                filter += $"and '{parentId}' in parents";
                //request.Fields = $"'{parentId}' in parents";
            }
            request.Q = filter;

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

        public string CreateNewFolder(string folderContainId, DriveService driveService, string folderName)
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder",
            };
            if (!string.IsNullOrEmpty(folderContainId))
            {
                fileMetadata.Parents = new List<string> { folderContainId };
            }
            FilesResource.CreateRequest request;

            request = driveService.Files.Create(fileMetadata);
            Google.Apis.Drive.v3.Data.File file = request.Execute();

            return file.Id;
        }

        public void CreateNewFile(string folderID, DriveService driveService, string filePath, string fileName)
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

            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                request = driveService.Files.Create(fileMetadata, stream, "application/pdf");
                request.Fields = "id";
                request.Upload();
            }

            //After create folder, get back folder to take folderID
            var file = request.ResponseBody;
            file.Shared = true;

            //share link will has format like below
            //https://drive.google.com/file/d/FILE_ID/view?usp=sharing
            //Console.WriteLine($"File link: https://drive.google.com/file/d/{file.Id}/view?usp=sharing");

            FileId = file.Id;
        }

        public void DeleteFile(DriveService service, string id)
        {
            //delete file
            //service.Files.Delete(id).Execute();

            //move file to trash
            service.Files.Update(new Google.Apis.Drive.v3.Data.File() { Trashed = true }, id).Execute();
        }
    }
}