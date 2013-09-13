using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ServiceModel.Web;
using System.ServiceModel.Channels;
using System.Xml.Linq;
using System.ServiceModel;
#if DEBUG
using WebOperationContext = System.ServiceModel.Web.MockedWebOperationContext;
#endif

namespace WebService
{
    [ServiceBehavior(UseSynchronizationContext = false)]
    public class WebService : IWebService
    {
        private readonly string errorPageTemplatePath;
        private readonly string contentSubPath;
        private readonly HtmlXmlFormatter htmlXmlFormatter = new HtmlXmlFormatter();


        public WebService(string errorPageTemplatePath, string contentSubPath)
        {
            this.contentSubPath = contentSubPath;
            this.errorPageTemplatePath = errorPageTemplatePath;
        }

        #region IWebService Members

        public Message ProvideClientAccessPolicy()
        {
            return Message.CreateMessage(MessageVersion.None, "",
                XDocument.Parse(htmlXmlFormatter.ClientAccessPolicyFile).CreateReader());
        }

        public Message ProvideCrossDomainPolicy()
        {
            return Message.CreateMessage(MessageVersion.None, "",
                XDocument.Parse(htmlXmlFormatter.CrossDomainPolicyFile).CreateReader());
        }

        public Stream GetFile(string path)
        {
            try
            {
                string contentType = null;
                int exIndex = path.LastIndexOf('.');
                string extension = path.Substring(exIndex, path.Length - exIndex);
                if ((contentType = getContentType(extension)) != null)
                {
                    WebOperationContext.Current.OutgoingResponse.ContentType = contentType;
                }
                if (!File.Exists(path))
                {
                    return getExceptionPage(new FileNotFoundException(path));
                }
                long offset = 0;
                long fileSize = (new FileInfo(path)).Length;
                var request = WebOperationContext.Current.IncomingRequest;
                var rangeKey = request.Headers["Range"];
                if (!string.IsNullOrEmpty(rangeKey))
                {
                    long.TryParse(rangeKey, out offset);
                    if (offset > fileSize - 1)
                    {
                        offset = fileSize - 1;
                    }
                }
                var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
                fileStream.Seek(offset, SeekOrigin.Begin);
                return fileStream;
            }
            catch (Exception ex)
            {
                return getExceptionPage(ex);
            }
        }

        public Stream GetContentList()
        {
            try
            {
                string output = htmlXmlFormatter.GetFileListAsTable(new DirectoryInfo(contentSubPath).GetFiles(), contentSubPath); //string.Concat("<table>", tableContent, "</table>");
                WebOperationContext.Current.OutgoingResponse.ContentType = "text/html;";
                return new MemoryStream(Encoding.UTF8.GetBytes(output));
            }
            catch (Exception ex)
            {
                return getExceptionPage(ex);
            }
        }

        public void UploadFile(Stream stream)
        {
            byte[] sub = null;
            string fileName = string.Empty;
            if ((sub = processStreamMetadata(stream, out fileName)) != null
                && !string.IsNullOrEmpty(fileName))
            {
                using (FileStream fstr = new FileStream(Path.Combine(contentSubPath, fileName),
                    FileMode.Create))
                {
                    int count;
                    byte[] buff = new byte[10000];
                    fstr.Write(sub, 0, sub.Length);
                    fstr.Flush();
                    while ((count = stream.Read(buff, 0, buff.Length)) > 0)
                    {
                        fstr.Write(buff, 0, count);
                        fstr.Flush();
                    }
                    fstr.Flush();
                    fstr.Close();
                }
            }
            WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.Redirect;
            WebOperationContext.Current.OutgoingResponse.Location =
                WebOperationContext.Current.IncomingRequest.Headers["Referer"];
        }

        #endregion

        private string getContentType(string extension)
        {
            switch (extension)
            {
                case ".html":
                case ".htm":
                    return "text/html";
                case ".js": return "text/javascript";
                case ".css": return "text/css";
                case ".png": return "image/png";
                case ".gif": return "image/gif";
                case ".jpg": return "image/jpg";
                case ".txt": return "text/html";
                case ".xap": return "application/x-silverlight-2-b2";
                case ".xml": return "text/xml";
                default: return null;
            }
        }

        private Stream getExceptionPage(Exception ex)
        {
            string exceptionBlock = htmlXmlFormatter.GetHTMLErrorMessage(ex);
            if (string.IsNullOrEmpty(errorPageTemplatePath))
            {
                return new MemoryStream(Encoding.Default.GetBytes(exceptionBlock));
            }

            WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";
            string exceptionPage = File.ReadAllText(errorPageTemplatePath);
            exceptionPage = exceptionPage.Replace("{}", exceptionBlock);
            return new MemoryStream(Encoding.Default.GetBytes(exceptionPage));
        }

        private byte[] processStreamMetadata(Stream inputStream, out string fileName)
        {
            fileName = string.Empty;
            int readed;
            byte[] buff = new byte[2048];
            while ((readed = inputStream.Read(buff, 0, 2048)) > 0)
            {
                string content = Encoding.UTF8.GetString(buff);
                int fileContentStringStart = content.IndexOf("\r\n\r\n");
                fileName = getUploadedFileName(content.Substring(0, fileContentStringStart));
                if (!string.IsNullOrEmpty(fileName))
                {
                    return buff.Skip(fileContentStringStart + 4).ToArray();
                }
            }
            return null;
        }

        private string getUploadedFileName(string metadata)
        {
            string fileName = getFileNameFromMetadata(metadata);
            string contentType = getContentTypeFromMetadata(metadata);
            if (string.IsNullOrEmpty(contentType))
            {
                return string.Empty;
            }
            return fileName;
        }

        private string getContentTypeFromMetadata(string metadata)
        {
            string contentType = string.Empty;
            if (metadata.ToLower().Contains("content-type"))
            {
                string tmp = metadata.Substring(metadata.ToLower().IndexOf("content-type:") + "Content-Type:".Length);
                contentType = tmp.Substring(0, (tmp.IndexOf("\r\n") == -1 ? tmp.Length : tmp.IndexOf("\r\n")));
            }
            return contentType;
        }

        private string getFileNameFromMetadata(string metadata)
        {
            string fileName = string.Empty;
            if (metadata.ToLower().Contains("filename"))
            {
                string tmp = metadata.Substring(metadata.ToLower().IndexOf("filename") + "filename".Length);
                tmp = tmp.Substring(tmp.IndexOf('"') + 1);
                fileName = tmp.Substring(0, tmp.IndexOf('"'));
                //FOR IE 1.6
                if (fileName.Contains('\\'))
                    fileName = fileName.Substring(fileName.LastIndexOf('\\') + 1);
            }
            return fileName;
        }
    }
}
