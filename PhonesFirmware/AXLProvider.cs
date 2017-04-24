using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PhonesFirmware
{
    class AXLProvider
    {
        public string CUCMIP { get; set; }
        public string CUCMPort { get; set; }
        public string axlUsername { get; set; }
        public string axlPassword { get; set; }
        public string axlVersion { get; set; }

        public AXLProvider(string CUCMIP, string CUCMPort, string axlUsername, string axlPassword,string axlVersion)
        {
            this.CUCMIP = CUCMIP;
            this.CUCMPort = CUCMPort;
            this.axlUsername = axlUsername;
            this.axlPassword = axlPassword;
            this.axlVersion = axlVersion;
        }

      

        public string executeQuery(string query)
        {
            try
            {

                

                System.Net.ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };

                HttpWebRequest httpRequest = (HttpWebRequest)HttpWebRequest.Create("https://"+CUCMIP+":"+CUCMPort+"/axl/");
                httpRequest.Method = "POST";
                httpRequest.Host = CUCMIP + ":" + CUCMPort;
                httpRequest.Accept = "text/*";
                httpRequest.ContentType = "text/xml";
                httpRequest.Headers.Add("Authorization", "Basic "+Convert.ToBase64String(Encoding.UTF8.GetBytes(axlUsername+":"+axlPassword)));
                httpRequest.Headers.Add("SOAPAction", "CUCM:DB ver="+axlVersion);

                string requestData = getRequestContent(query);

                httpRequest.ContentLength = requestData.Length;

                var streamWriter = new StreamWriter(httpRequest.GetRequestStream());
                streamWriter.Write(requestData);
                streamWriter.Close();

                using (Stream stream = httpRequest.GetResponse().GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                    String responseString = reader.ReadToEnd();
                    return responseString;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string getRequestContent(string query)
        {
            return "<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:ns=\"http://www.cisco.com/AXL/API/" + axlVersion + "\">"
                                    + "<SOAP-ENV:Body>"
                                    + "<ns:executeSQLQuery sequence=\"1\">"
                                    + "<sql>"
                                    + query
                                    + "</sql>"
                                    + "</ns:executeSQLQuery>"
                                    + "</SOAP-ENV:Body>"
                                    + "</SOAP-ENV:Envelope>";
        }
    }
}
