using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace WebService
{
    public class HtmlXmlFormatter
    {

        private readonly string clientAccessPolicyFile = @"<?xml version=""1.0"" encoding=""utf-8""?>
                                                <access-policy>
                                                  <cross-domain-access>
                                                    <policy>
                                                      <allow-from http-request-headers=""*"">
                                                        <domain uri=""*""/>
                                                      </allow-from>
                                                      <grant-to>
                                                        <resource path=""/"" include-subpaths=""true""/>
                                                      </grant-to>
                                                    </policy>
                                                  </cross-domain-access>
                                                </access-policy>";

        private readonly string crossDomainPolicyFile = @"<?xml version=""1.0""?>
                                            <!DOCTYPE cross-domain-policy>
                                            <cross-domain-policy>
                                              <allow-http-request-headers-from domain=""*"" headers=""*""/>
                                            </cross-domain-policy>";

        public string ClientAccessPolicyFile
        {
            get
            {
                return clientAccessPolicyFile;
            }
        }

        public string CrossDomainPolicyFile
        {
            get
            {
                return crossDomainPolicyFile;
            }
        }

        public string GetFileListAsTable(IEnumerable<FileInfo> filesInfo, string contentSubPath)
        {
            string tableContent = string.Concat(
                from fileInfo in filesInfo
                select
                String.Format(@"<tr><td><div class=""fileInfo""><a class=""target_silverlight"" href=""#{0}"">{1}</a><div/><td/></tr>",
                            Path.Combine(contentSubPath, fileInfo.Name).Replace(@"\", @"/"),
                            fileInfo.Name)
                );
            string output = string.Concat("<table>", tableContent, "</table>");
            return output;
        }

        public string GetHTMLErrorMessage(Exception exception)
        {
            return String.Format(@"<div>
	    	<p>Message:</p>	
	    	<h4>{0}</h4>    	
	    	<p>Details:</p>
	    	<h4>{1}</h4>    
    	    </div> ", exception.Message, exception.StackTrace);
        }

    }
}
