// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace gip.tool.installerAndUpdater
{
    public class HttpClientDownloadWithProgress : IDisposable
    {
        private readonly string _downloadUrl;
        private readonly string _destinationFilePath;

        private HttpClient _httpClient;

        public delegate void ProgressChangedHandler(long? totalFileSize, long totalBytesDownloaded, double? progressPercentage);

        public event ProgressChangedHandler ProgressChanged;

        public HttpClientDownloadWithProgress(string downloadUrl, string destinationFilePath)
        {
            _downloadUrl = downloadUrl;
            _destinationFilePath = destinationFilePath;
        }

        public async Task StartDownload()
        {
            _httpClient = new HttpClient { Timeout = TimeSpan.FromDays(1) };

            var values = new Dictionary<string, string>
                {
                   { "username", LoginManager.Username },
                   { "password", LoginManager.Password }
                };
            var content = new FormUrlEncodedContent(values);
            var responseGetUserStatus = await _httpClient.PostAsync(LoginManager.UserStatusUrl, content);

            var responseString = await responseGetUserStatus.Content.ReadAsStringAsync();

            UserStatusInfo info = JsonConvert.DeserializeObject<UserStatusInfo>(responseString);
            if (info.IsUsernamePassOk)
            {
                using (var response = await _httpClient.GetAsync(_downloadUrl, HttpCompletionOption.ResponseHeadersRead))
                    await DownloadFileFromHttpResponseMessage(response);
            }
        }

        public async Task StartDownload(CancellationToken cancelToken)
        {
            if (cancelToken.IsCancellationRequested)
                return;

            _httpClient = new HttpClient { Timeout = TimeSpan.FromDays(1) };

            var values = new Dictionary<string, string>
                {
                   { "username", LoginManager.Username },
                   { "password", LoginManager.Password }
                };
            var content = new FormUrlEncodedContent(values);

            if (cancelToken.IsCancellationRequested)
            {
                _httpClient.Dispose();
                return;
            }

            var responseGetUserStatus = await _httpClient.PostAsync(LoginManager.UserStatusUrl, content, cancelToken);
            if (cancelToken.IsCancellationRequested)
            {
                _httpClient.Dispose();
                return;
            }
            var responseString = await responseGetUserStatus.Content.ReadAsStringAsync();
            if (cancelToken.IsCancellationRequested)
            {
                _httpClient.Dispose();
                return;
            }

            UserStatusInfo info = JsonConvert.DeserializeObject<UserStatusInfo>(responseString);
            if (info.IsUsernamePassOk)
            {
                using (var response = await _httpClient.GetAsync(_downloadUrl, HttpCompletionOption.ResponseHeadersRead, cancelToken))
                    await DownloadFileFromHttpResponseMessage(response, cancelToken);
            }
        }

        private async Task DownloadFileFromHttpResponseMessage(HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength;

            using (var contentStream = await response.Content.ReadAsStreamAsync())
                await ProcessContentStream(totalBytes, contentStream);
        }

        private async Task DownloadFileFromHttpResponseMessage(HttpResponseMessage response, CancellationToken cancelToken)
        {
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength;

            using (var contentStream = await response.Content.ReadAsStreamAsync())
                await ProcessContentStream(totalBytes, contentStream, cancelToken);
        }

        private async Task ProcessContentStream(long? totalDownloadSize, Stream contentStream)
        {
            var totalBytesRead = 0L;
            var readCount = 0L;
            var buffer = new byte[1024];
            var isMoreToRead = true;

            using (var fileStream = new FileStream(_destinationFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 1024, true))
            {
                do
                {
                    int bytesRead = 0;
                    try
                    {
                        bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                    }
                    catch
                    {
                        fileStream.Dispose();
                    }
                    if (bytesRead == 0)
                    {
                        isMoreToRead = false;
                        TriggerProgressChanged(totalDownloadSize, totalBytesRead);
                        continue;
                    }

                    try
                    {
                        await fileStream.WriteAsync(buffer, 0, bytesRead);
                    }
                    catch
                    {
                        fileStream.Dispose();
                    }

                    totalBytesRead += bytesRead;
                    readCount += 1;

                    if (readCount % 100 == 0)
                        TriggerProgressChanged(totalDownloadSize, totalBytesRead);
                }
                while (isMoreToRead);
            }
        }

        private async Task ProcessContentStream(long? totalDownloadSize, Stream contentStream, CancellationToken cancelToken)
        {
            var totalBytesRead = 0L;
            var readCount = 0L;
            var buffer = new byte[1024];
            var isMoreToRead = true;

            using (var fileStream = new FileStream(_destinationFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 1024, true))
            {
                do
                {
                    int bytesRead = 0;
                    try
                    {
                        bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancelToken);
                    }
                    catch
                    {
                        fileStream.Dispose();
                        return;
                    }
                    if (bytesRead == 0)
                    {
                        isMoreToRead = false;
                        TriggerProgressChanged(totalDownloadSize, totalBytesRead);
                        continue;
                    }

                    try
                    {
                        await fileStream.WriteAsync(buffer, 0, bytesRead, cancelToken);
                    }
                    catch
                    {
                        fileStream.Dispose();
                        return;
                    }

                    totalBytesRead += bytesRead;
                    readCount += 1;

                    if (readCount % 100 == 0)
                        TriggerProgressChanged(totalDownloadSize, totalBytesRead);
                }
                while (isMoreToRead);
            }
        }

        private void TriggerProgressChanged(long? totalDownloadSize, long totalBytesRead)
        {
            if (ProgressChanged == null)
                return;

            double? progressPercentage = null;
            if (totalDownloadSize.HasValue)
                progressPercentage = Math.Round((double)totalBytesRead / totalDownloadSize.Value * 100, 2);

            ProgressChanged(totalDownloadSize, totalBytesRead, progressPercentage);
        }

        public void Dispose()
        {
            if(_httpClient != null)
                _httpClient.Dispose();
        }
    }
}
