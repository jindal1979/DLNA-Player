﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.IO;
using System.Threading;

namespace DLNAPlayer
{
    public class GDrive
    {
        static readonly string[] Scopes = { DriveService.Scope.DriveReadonly };
        static string SoftwareName = "DLNA Player";
        public DriveService service;
        public List<string> FolderList = new List<string> { };
        public List<string> FolderListID = new List<string> { };
        public List<string> FileList = new List<string> { };
        public List<string> FileListID = new List<string> { };
        public string currentFolder = "";
        public string currentFolderName = "";
        public string previousFolder = "";
        public GDrive()
        {
            UserCredential credential;
            using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/DLNAPlayer.json");
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(GoogleClientSecrets.Load(stream).Secrets, Scopes, "user", CancellationToken.None, new FileDataStore(credPath, true)).Result;
            }
            service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = SoftwareName,
            });
        }

        public void GetData(string folderId)
        {
            FolderList.Clear();
            FolderListID.Clear();
            FileList.Clear();
            FileListID.Clear();
            string listRequestQString = "mimeType!='application/vnd.google-apps.folder' and '" + folderId + "' in parents and trashed = false";
            string listRequestQFolderString = "mimeType='application/vnd.google-apps.folder' and '" + folderId + "' in parents and trashed = false";
            string PageToken1 = string.Empty;
            do
            {
                FilesResource.ListRequest listRequest = service.Files.List();
                listRequest.Fields = "nextPageToken, files(id, name)";
                listRequest.Q = listRequestQString;
                listRequest.OrderBy = "name";
                listRequest.PageToken = PageToken1;
                try
                {
                    var files = listRequest.Execute();
                    if (files.Files != null && files.Files.Count > 0)
                    {
                        foreach (var file in files.Files)
                        {
                            FileList.Add(file.Name);
                            FileListID.Add(file.Id);
                        }
                    }
                    PageToken1 = files.NextPageToken;
                }
                catch { }
            } while (PageToken1 != null);
            string PageToken2 = string.Empty;
            do
            {
                FilesResource.ListRequest listRequest = service.Files.List();
                listRequest.Fields = "nextPageToken, files(id, name)";
                listRequest.Q = listRequestQFolderString;
                listRequest.OrderBy = "name";
                listRequest.PageToken = PageToken2;
                try
                {
                    var files = listRequest.Execute();
                    if (files.Files != null && files.Files.Count > 0)
                    {
                        foreach (var file in files.Files)
                        {
                            FolderList.Add(file.Name);
                            FolderListID.Add(file.Id);
                        }
                    }
                    PageToken2 = files.NextPageToken;
                }
                catch { }
            } while (PageToken2 != null);
            previousFolder = currentFolder;
            currentFolder = folderId;
            currentFolderName = GetFolderName(currentFolder);
        }

        private string GetFolderName(string Id)
        {
            try
            {
                FilesResource.GetRequest getRequest = service.Files.Get(Id);
                Google.Apis.Drive.v3.Data.File folderName = getRequest.Execute();
                return folderName.Name;
            }
            catch
            {
                return "Error retrieving folder name";
            }
        }
        public async Task<MemoryStream> DownloadFile(string Id)
        {
            MemoryStream downloadedFile = new MemoryStream();
            FilesResource.GetRequest getRequest = service.Files.Get(Id);
            await getRequest.DownloadAsync(downloadedFile);
            return downloadedFile;
        }

    }
}
