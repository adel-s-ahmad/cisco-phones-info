using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using PhonesFirmware.GetDevicesByName;
using System.Xml;

namespace PhonesFirmware
{
    class Device
    {
        public string name { get; set; }
        public string description { get; set; }
    }

    class RisServiceConsumer
    {
        public List<Device> getPhonesAXL()
        {
            List<CmDevice> resultDevices = new List<CmDevice>();

             try
            {
                string[] cucms = Properties.Settings.Default.IP.Split(',');

                AXLProvider axl = new AXLProvider(cucms[0], Properties.Settings.Default.Port, Properties.Settings.Default.UserId, Properties.Settings.Default.Password, Properties.Settings.Default.Version);
                string result = axl.executeQuery("select name,description from device where tkclass = '1'");

                XDocument doc = XDocument.Parse(result);
                List<Device> phones = doc.Descendants("row").Select(p => new Device { name = p.Element("name").Value, description = p.Element("description").Value }).ToList<Device>();

                return phones;
                //Console.WriteLine("number of nodes returned from AXL: " + phones.Count());

                //double loopCount = Math.Ceiling((double)phones.Count() / (double)200);

                //for (int i = 0; i < loopCount; i++)
                //{
                //    var desiredPhones = phones.Skip(i * 200).Take(200);

                //    SelectItem[] selectItems = new SelectItem[desiredPhones.Count()];
                //    int counter = 0;
                //    foreach (var phone in desiredPhones)
                //    {
                //        SelectItem item = new SelectItem();
                //        item.Item = phone.name;
                //        selectItems[counter] = item;
                //        counter++;
                //    }

                //    var client = new RISService(Properties.Settings.Default.IP,Properties.Settings.Default.Port);

                //    //specify the application user credentials (this user must have AXL administration role in the CUCM)
                //    NetworkCredential cred = new NetworkCredential(Properties.Settings.Default.UserId, Properties.Settings.Default.Password);
                //    client.UseDefaultCredentials = false;
                //    client.Credentials = cred;


                //    //specify the search criteria 
                //    CmSelectionCriteria query = new CmSelectionCriteria();
                //    query.Class = "Phone";
                //    query.Model = 255;
                //    query.SelectBy = "Name";
                //    query.MaxReturnedDevices = 200;

                //    //specify the name of the devices to search for.
                //    query.SelectItems = selectItems;

                //    //to be populated by the WebService with the request state
                //    string State = "";

                //    //to add the server certificate to the request
                //    System.Net.ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };

                //    //the actual request 
                //    GetDevicesByName.SelectCmDeviceResult phoneDetails = client.SelectCmDevice(ref State, query);
                //    Console.WriteLine("Total devices found: " + phoneDetails.TotalDevicesFound);
                //    //parsing the response and view the formatted result
                //    if (phoneDetails.CmNodes != null)
                //    {
                //        foreach (GetDevicesByName.CmNode node in phoneDetails.CmNodes)
                //        {
                //            resultDevices.AddRange(node.CmDevices.ToList());
                //        }
                //    }


                //}

                //return resultDevices;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<CmDevice> getPhonesDetails(List<Device> phones)
        {
            List<CmDevice> resultDevices = new List<CmDevice>();

            try
            {
                double loopCount = Math.Ceiling((double)phones.Count() / (double)200);

                for (int i = 0; i < loopCount; i++)
                {
                    var desiredPhones = phones.Skip(i * 200).Take(200);

                    SelectItem[] selectItems = new SelectItem[desiredPhones.Count()];
                    int counter = 0;
                    foreach (var phone in desiredPhones)
                    {
                        SelectItem item = new SelectItem();
                        item.Item = phone.name;
                        selectItems[counter] = item;
                        counter++;
                    }

                    string[] cucms = Properties.Settings.Default.IP.Split(',');
                    foreach (var ip in cucms)
                    {


                        var client = new RISService(ip, Properties.Settings.Default.Port);

                        //specify the application user credentials (this user must have AXL administration role in the CUCM)
                        NetworkCredential cred = new NetworkCredential(Properties.Settings.Default.UserId, Properties.Settings.Default.Password);
                        client.UseDefaultCredentials = false;
                        client.Credentials = cred;


                        //specify the search criteria 
                        CmSelectionCriteria query = new CmSelectionCriteria();
                        query.Class = "Phone";
                        query.Model = 255;
                        query.SelectBy = "Name";
                        query.MaxReturnedDevices = 200;

                        //specify the name of the devices to search for.
                        query.SelectItems = selectItems;

                        //to be populated by the WebService with the request state
                        string State = "";

                        //to add the server certificate to the request
                        System.Net.ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };

                        //the actual request 
                        GetDevicesByName.SelectCmDeviceResult phoneDetails = client.SelectCmDevice(ref State, query);
                        //Console.WriteLine("Total devices found: " + phoneDetails.TotalDevicesFound);
                        //parsing the response and view the formatted result
                        if (phoneDetails.CmNodes != null)
                        {
                            foreach (GetDevicesByName.CmNode node in phoneDetails.CmNodes)
                            {
                                resultDevices.AddRange(node.CmDevices.Where(d => d.Status == CmDevRegStat.Registered && !resultDevices.Any(rd => rd.IpAddress == d.IpAddress && rd.Name == d.Name) ).ToList());
                            }
                        }

                    }
                }

                return resultDevices;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public PhoneInfo getPhoneInfo(CmDevice phone)
        {
            PhoneInfo resultDevice = new PhoneInfo();
            try
            {
                resultDevice.device = phone;
                string url = "http://"+phone.IpAddress+"/DeviceInformationX";
                WebRequest request = WebRequest.Create(url);

                using (WebResponse response = request.GetResponse())
                {
                    StreamReader sr = new StreamReader(response.GetResponseStream());
                    var responseStr = sr.ReadToEnd();
                    XDocument doc = XDocument.Parse(responseStr);
                    //resultDevice.version = doc.Descendants("versionID").Select(v => v.Value).FirstOrDefault();
                    resultDevice.info = doc.Descendants().ToList();
                }

                return resultDevice;
            }
            catch(WebException wex)
            {
                return null;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public List<PhoneInfo> getPhonesInfo(List<CmDevice> devices)
        {
            List<PhoneInfo> result = new List<PhoneInfo>();
            try
            {
                foreach(CmDevice device in devices)
                {
                    PhoneInfo deviceInfo = getPhoneInfo(device);
                    if(deviceInfo!=null)
                        result.Add(deviceInfo);
                }
                return result;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
    }
}
