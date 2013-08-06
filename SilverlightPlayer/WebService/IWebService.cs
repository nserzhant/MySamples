using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.IO;
using System.ServiceModel.Channels;

namespace WebService
{
    [ServiceContract(SessionMode = SessionMode.NotAllowed)]
    public interface IWebService
    {

        [OperationContract, WebGet(UriTemplate = "ClientAcessPolicy.xml")]
        Message ProvideClientAccessPolicy();

        [OperationContract, WebGet(UriTemplate = "crossdomain.xml")]
        Message ProvideCrossDomainPolicy();

        [WebGet(UriTemplate = "Filelist")]
        Stream GetContentList();

        [WebInvoke(Method = "POST", UriTemplate = "/upload", BodyStyle = WebMessageBodyStyle.Bare)]
        void UploadFile(Stream stream);

        [WebGet(UriTemplate = "{*path}")]
        Stream GetFile(string path);
    }
}
