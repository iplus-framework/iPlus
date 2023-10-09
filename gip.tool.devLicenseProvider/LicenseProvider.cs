using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gip.core.datamodel;
using System.Security.Cryptography;
using System.IO;
using System.Runtime.Serialization;
using MXAPI;
using System.Collections.ObjectModel;
using System.Xml;
using Microsoft.EntityFrameworkCore;


namespace gip.tool.devLicenseProvider
{
    public class LicenseProvider
    {
        #region c'tors

        public LicenseProvider()
        {
            CommonPrivateKey = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "\\CommonPrivateKey.xml");
            if (string.IsNullOrEmpty(CommonPrivateKey))
                throw new Exception("Can't load common private key.");
        }

        #endregion

        #region Properties

        private const int _UserCode = 4228216;

        private const short _Port = 85;

        private string CommonPrivateKey
        {
            get;
            set;
        }

        private ObservableCollection<License> _Licenses;
        public ObservableCollection<License> Licenses
        {
            get
            {
                if (_Licenses == null)
                {
                    using (var ctx = new GIPLicenseEntities())
                    {
                        _Licenses = new ObservableCollection<License>(ctx.License.Include("Customer").OrderBy(c => c.LicenseNo).ToList());
                    }
                }
                return _Licenses;
            }
        }

        private ObservableCollection<Customer> _Customers;
        public ObservableCollection<Customer> Customers
        {
            get
            {
                if (_Customers == null)
                {
                    using (var ctx = new GIPLicenseEntities())
                    {
                        _Customers = new ObservableCollection<Customer>(ctx.Customer.OrderBy(c => c.CustomerNo).ToList());
                    }
                }
                return _Customers;
            }
        }

        private List<string> _AvailablePackages;
        public List<string> AvailablePackages
        {
            get
            {
                if (_AvailablePackages == null)
                {
                    using(var ctx = new iPlusV5Context())
                    {
                        _AvailablePackages = ctx.ACPackage.Where(c => c.ACPackageName != Const.PackName_VarioDevelopment && c.ACPackageName != Const.PackName_VarioSystem &&
                                                                      c.ACPackageName != Const.PackName_System).Select(c => c.ACPackageName)
                                                          .Intersect(ACPackage.VBPackages.Select(x => x.Item1)).ToList();
                    }
                }
                return _AvailablePackages;
            }
        }

        public VBSystem CurrentVBSystem
        {
            get;
            set;
        }

        public string DatabaseName
        {
            get
            {
                //if (CurrentVBSystem != null)
                //    return CurrentVBSystem.SystemInfo1;
                return "";
            }
        }

        public string DataSource
        {
            get
            {
                //if (CurrentVBSystem != null)
                //    return CurrentVBSystem.SystemInfo2;
                return "";
            }
        }

        public string Package
        {
            get
            {
                if (CurrentVBSystem != null)
                    return Encoding.UTF8.GetString(CurrentVBSystem.SystemInternal);
                return "";
            }
        }

        #endregion

        #region Methods

        public bool GenerateLicenseFile(Customer customer, string licensePath, string projectNo, License currentLicense = null)
        {
            if (currentLicense == null)
            {
                using (var ctx = new GIPLicenseEntities())
                {
                    if (ctx.License.Any(c => c.ProjectNo == projectNo))
                    {
                        return false;
                    }
                }
            }
            if (string.IsNullOrEmpty(projectNo) || customer == null)
                return false;

            VBSystem vbSystem = new VBSystem
            {
                VBSystemID = Guid.NewGuid(),
                SystemName = "VBD",
                CustomerName = customer.CustomerName,
                ProjectNo = projectNo,
                SystemCommonPublic = "0x32",
                Company = "gip"
            };

            License license = null;
            if(currentLicense != null)
            {
                license = currentLicense;
                license.Customer = null;
            }
            else
            {
                license = new License
                {
                    LicenseID = Guid.NewGuid(),
                    CustomerID = customer.CustomerID,
                    LicenseNo = GetLicenseNo(),
                    ProjectNo = projectNo
                };
            }

            byte[] pass = new byte[60];
            Random rnd = new Random();
            rnd.NextBytes(pass);

            if (license.DongleKey != null)
                pass = license.DongleKey;

            byte[] code;
            using (SHA256 sha = SHA256.Create())
            {
                code = sha.ComputeHash(Encoding.UTF8.GetBytes(pass.ToByteStringKey()));
            }

            string cipher = Encrypt.EncryptString(customer.CustomerName, code);
            vbSystem.SystemInternal = Encoding.UTF8.GetBytes(cipher);

            RSACryptoServiceProvider csp = new RSACryptoServiceProvider(2048);
            vbSystem.SystemInternal2 = csp.ToXmlString(false);
            vbSystem.SystemInternal1 = csp.SignData(vbSystem.SystemInternal, SHA256.Create());
            license.PackagePrivateKey = csp.ToXmlString(true);

            csp = new RSACryptoServiceProvider(2048);
            vbSystem.SystemPrivate = csp.ToXmlString(true);

            DSACryptoServiceProvider dsa = new DSACryptoServiceProvider();
            license.RemotePrivateKey = dsa.ToXmlString(true);
            vbSystem.SystemRemote = dsa.ToXmlString(false);

            csp = new RSACryptoServiceProvider();
            csp.FromXmlString(CommonPrivateKey);
            vbSystem.SystemCommon = csp.SignData(vbSystem.GetChecksum(), SHA256.Create());

            DataContractSerializer serializer = new DataContractSerializer(typeof(VBSystem));
            string licenceText = "";
            using (MemoryStream ms = new MemoryStream())
            {
                serializer.WriteObject(ms, vbSystem);
                licenceText = Encoding.Default.GetString(ms.ToArray());
            }

            string filePath = string.Format(@"{0}\{1}-devLicense.gip", licensePath, customer.CustomerName);
            File.WriteAllText(filePath, licenceText);
            license.VBSystemKey = licenceText;
            license.DongleKey = pass;

            if(currentLicense == null)
            {
                using (var ctx = new GIPLicenseEntities())
                {
                    ctx.License.Add(license);
                    ctx.SaveChanges();
                }
                license.Customer = customer;
                Licenses.Add(license);
            }

            GenerateDongleLicence(GetAvailableDongles(), pass);

            return true;
        }

        public void DeserializeLicence(string licenceXml)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(VBSystem));
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(licenceXml)))
            {
                CurrentVBSystem = serializer.ReadObject(ms) as VBSystem;
            }
        }

        public string GenerateCustomerPackageLicense(IEnumerable<string> packages, string customerCode, string remotePrivateKey)
        {
            if (packages == null || string.IsNullOrEmpty(customerCode) || string.IsNullOrEmpty(remotePrivateKey))
                return "Invalid parameters!!!";

            string key = "";
            using (var dsa = new DSACryptoServiceProvider())
            {
                try
                {
                    dsa.FromXmlString(remotePrivateKey);
                    key = dsa.SignData(Encoding.UTF8.GetBytes(customerCode)).ToByteStringKey();
                }
                catch
                {
                    return "Invalid remote private key!!!";
                }
            }

            key += "&";

            foreach(string package in packages)
            {
                short packageNo = ACPackage.VBPackages.FirstOrDefault(c => c.Item1 == package).Item2;
                key += packageNo.ToString() + "-";
            }

            key = key.TrimEnd('-');

            return key;
        }

        public List<DongleInfo> GetAvailableDongles()
        {
            if (Environment.Is64BitProcess)
                return GetAvailableDongles64();
            return GetAvailbledDongles32();
        }

        private List<DongleInfo> GetAvailbledDongles32()
        {
            short dngCount = Matrix.Dongle_Count(_Port);
            if (dngCount < 0)
                return null;

            List<DongleInfo> tempList = new List<DongleInfo>();
            for (short i = 1; i <= dngCount; i++)
            {
                tempList.Add(new DongleInfo() { DongleNo = i, DongleSerialNo = Matrix.Dongle_ReadSerNr(_UserCode, i, _Port) });
            }
            return tempList;
        }

        private List<DongleInfo> GetAvailableDongles64()
        {
            short dngCount = Matrix.Dongle_Count64(_Port);
            if (dngCount < 0)
                return null;

            List<DongleInfo> tempList = new List<DongleInfo>();
            for (short i = 1; i <= dngCount; i++)
            {
                tempList.Add(new DongleInfo() { DongleNo = i, DongleSerialNo = Matrix.Dongle_ReadSerNr64(_UserCode, i, _Port) });
            }
            return tempList;
        }

        public string GenerateDongleLicence(IEnumerable<DongleInfo> availableDongles, byte[] key)
        {
            int[] arr = new int[15];
            using(DSACryptoServiceProvider dsa = new DSACryptoServiceProvider())
            {
                try
                {
                    for (int i = 0; i < 15; i++)
                    {
                        arr[i] = BitConverter.ToInt32(key.Skip(i * 4).Take(4).ToArray(), 0);
                    }
                }
                catch
                {
                    return "Error: Can't create developer licence!!!";
                }
            }
            
            string msg = "";

            foreach(DongleInfo info in availableDongles)
            {
                if(Environment.Is64BitProcess)
                {
                    if (!GenerateDongleLicnece64(info, arr))
                        msg += string.Format("Error: Can't write developer licence - usb dongle serial no: {0} {1}", info.DongleSerialNo, Environment.NewLine);
                }
                else
                {
                    if(!GenerateDongleLicence32(info, arr))
                        msg += string.Format("Error: Can't write developer licence - usb dongle serial no: {0} {1}", info.DongleSerialNo, Environment.NewLine);
                }
            }

            if (string.IsNullOrEmpty(msg))
                msg = "Development licensing is  succesfully completed.";

            return msg;
        }

        private bool GenerateDongleLicence32(DongleInfo info, int[] data)
        {
            try
            {
                Matrix.Dongle_WriteData(_UserCode, ref data[0], 15, info.DongleNo, _Port);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool GenerateDongleLicnece64(DongleInfo info, int[] data)
        {
            try
            {
                Matrix.Dongle_WriteData64(_UserCode, ref data[0], 15, info.DongleNo, _Port);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string ClearDongle(IEnumerable<DongleInfo> availableDongles)
        {
            int[] arr = new int[15];
            string msg = "";
            foreach(var info in availableDongles)
            {
                try
                {
                    if (Environment.Is64BitProcess)
                        Matrix.Dongle_WriteData64(_UserCode, ref arr[0], 15, info.DongleNo, _Port);
                    else
                        Matrix.Dongle_WriteData(_UserCode, ref arr[0], 15, info.DongleNo, _Port);
                }
                catch
                {
                    msg += string.Format("Error: Can't clear dongle - serial no: {0} {1}", info.DongleSerialNo, Environment.NewLine);
                }
            }
            if (string.IsNullOrEmpty(msg))
                msg = "License clearing process is succesfully completed.";

            return msg;
        }

        public string GenerateRemoteLoginKey(string userCode, string remoteLoginPrivateKey)
        {
            string remoteLoginKey = "";
            using (var dsa = new DSACryptoServiceProvider())
            {
                dsa.FromXmlString(remoteLoginPrivateKey);
                var signature = dsa.SignData(Encoding.UTF8.GetBytes(userCode));
                remoteLoginKey = signature.ToByteStringKey();
            }
            return remoteLoginKey;
        }

        public bool DeleteLicense(License currentLicence)
        {
            using (var ctx = new GIPLicenseEntities())
            {
                var licence = ctx.License.FirstOrDefault(c => c.LicenseID == currentLicence.LicenseID);
                if (licence != null)
                {
                    try
                    {
                        ctx.License.Remove(licence);
                        ctx.SaveChanges();
                        Licenses.Remove(currentLicence);
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public bool DeleteCustomer(Customer currentCustomer)
        {
            using (var ctx = new GIPLicenseEntities())
            {
                var customer = ctx.Customer.FirstOrDefault(c => c.CustomerID == currentCustomer.CustomerID);
                if (customer != null)
                {
                    try
                    {
                        ctx.Customer.Remove(customer);
                        ctx.SaveChanges();
                        Customers.Remove(currentCustomer);
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public string CreateNewCustomer(string customerName, string customerAddress)
        {
            using (var ctx = new GIPLicenseEntities())
            {
                if (string.IsNullOrEmpty(customerName) || string.IsNullOrEmpty(customerAddress))
                    return "Customer name or customer address is empty!!!";

                if (ctx.Customer.Any(c => c.CustomerName == customerName))
                    return string.Format("Customer with name: {0} allready exist!!!", customerName);

                Customer cust = new Customer
                {
                    CustomerID = Guid.NewGuid(),
                    CustomerNo = GetCustomerNo(),
                    CustomerName = customerName,
                    Address = customerAddress
                };

                ctx.Customer.Add(cust);
                ctx.SaveChanges();
                Customers.Add(cust);
            }
            return null;
        }

        private int GetCustomerNo()
        {
            int custNo = 0;
            using(var ctx = new GIPLicenseEntities())
            {
                if(ctx.Customer.Any())
                    custNo = ctx.Customer.Max(c => c.CustomerNo);
            }
            custNo++;
            return custNo;
        }

        private int GetLicenseNo()
        {
            int licenceNo = 0;
            using(var ctx = new GIPLicenseEntities())
            {
                if(ctx.License.Any())
                    licenceNo = ctx.License.Max(c => c.LicenseNo);
            }
            licenceNo++;
            return licenceNo;
        }

        public bool DeserializeActivationData(string xml, out Customer customer, out string database, out string datasource, out string code)
        {
            customer = null;
            database = "";
            datasource = "";
            code = "";

            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(xml);
                foreach (XmlNode node in doc.SelectSingleNode("xml").ChildNodes)
                {
                    switch (node.Name)
                    {
                        case "companyName":
                            customer = GetOrCreateCustomer(node.InnerText);
                            break;

                        case "db":
                            database = node.InnerText;
                            break;

                        case "ds":
                            datasource = node.InnerText;
                            break;

                        case "code":
                            code = node.InnerText;
                            break;
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        private Customer GetOrCreateCustomer(string customerName)
        {
            Customer customer = Customers.FirstOrDefault(c => c.CustomerName == customerName);
            if(customer == null)
            {
                CreateNewCustomer(customerName, "");
                customer = Customers.FirstOrDefault(c => c.CustomerName == customerName);
            }

            return customer;
        }

        #region Server methods
        public string SingLicenseFile(string input)
        {
            try
            {
                byte[] data = ToByteArray(input);
                RSACryptoServiceProvider csp = new RSACryptoServiceProvider();
                csp.FromXmlString("<RSAKeyValue><Modulus>3ZgedYpQrgIFSvW+77uVEZPObwbCSEtlzbp/hiY9rWK3+kWtopf5AZWweSiZ8+AUVp2jV7WQwsmvdO/kJ/86/U3EcRbNsfJPJDDYwjWdGP3+tG5FB0tGHc/eUlfa762cxm7bf0AZ76zkz19xnzpbNVWSAKquc+0YAhdl7b4jqlBrQ2ydogJkDMLPU1GF8hRTpIzEr951KFA+Ss0opH3JllCQL4WTlOqQDZje9DlUu64hIX8M7vRwHZblHAZtO+MT6bKgP43SYiCYo7KxYDyFpSl1Xh5GQsRL8Qcqhnr+Bq7DlzpUMsErkCWecR4iq0fMnIz7FEpHedYZ1LjDoZTKaQ==</Modulus><Exponent>AQAB</Exponent><P>7ThGyUW4P5vr4rcwiI2rKkkKQZPIjf3zZvnVr3ZXnxEbqJP4ib+qzKk0p4249HZ3D1XNFbNcQhxwGcFVb+U7Kz7dvkJzOXEzBKTlZt2FxMJPs1pgkiQacglW6D4R2vxQrotjk/++2Q2aEL7bIJavCZpPS6HKJU4mH5/a0SUCH9M=</P><Q>7yMoqZrbLlef8tsG26ltRZz/nb0P2r+Y4k/LMC6rvwmQyZh+nCkL5OxeqDKxZRPTa5J00bfKs8EvEeKLkVI4e0XvzS5BEuiY3VEAutTDjH1zM4jSuBBSFEb8DuniPineNQxijWnn8CYmQstHdmXOcZnEBDWfhrqQqmE6YRhHA1M=</Q><DP>GIFBvrT3DYsb2PW3i8OmtN2Ks6+CfjiHllGko1WEQ6hOxSFUAVbNXAr2p4BaZNaAAhOI9f7rPuEVK3PvUXnKnPMHkQnoQTzSWl52XCPyF5tDBHIHm9Ei8jjIw4D18zsxUnaPuNAodN+U1LoChOFL/5/zJQr3iNcD1Sx8PDKof4U=</DP><DQ>IQPhoUjX6dX/JzBGCh2iEHJUeBqaDcFWAiiyDLzkyUMw0iRTlou0MK7Rgrc89o9+KOPXbPzK53ZMYVO9oRqQ4bQOH227Xjjuod+FEkY9mS/Yr8y8Ct1194a1VfnEWoC9ROWo1Y1BkE40ChS2kQoNLnHkNhCRLbCkOGGkBbOsWjE=</DQ><InverseQ>lbrEkcpR0LHgGgAnD5aoIf7qBzzD1ybSr4mFMbC7RUovjDscWOJdmCocmV1ac2OcsCNciotWSGEEJI3jfR5rLRZ63wVOn9f3HjoVxptCkPJl4T0dxbE+yD/kc1QM5z4l3ava0l5CNGlKjZVIMPe3SdT6k4TFKefEjvw3R+NZkHQ=</InverseQ><D>StfRGdQAkfT+wqWjuqa5n3kzlQ5MWkyU8tpVrgKGfGRGTVJxZeQ8Zwue0h0jeloppGOTwtEBNrkV+MH5ZoTu8JTuj1+rU7nKfye8XkPrboCDIX/I8sC6yuDlbxxbRu51cBQLMLx+xhO4KE5NOwFjwuzG5lC/oUnn/PTYKWc59pCX9cTtRu3qUvPdgRoJYHUpI7RkMicB1KGgw6eodBlt6hGxTUj3Q900+4atKktzTeghQV3kqNKiwq4xx2GkTQ8xfI1e7MpNWcz68lKQCKmG/eSYpO1u+o0so62ZWAjpjuXByy785C10IU7Y8JDvSbbtrgLCEkquVZPEy2+IcL6YnQ==</D></RSAKeyValue>");
                var sign = csp.SignData(data, SHA256.Create());
                return ToByteString(sign);
            }
            catch
            {
                return "";
            }
        }

        private byte[] ToByteArray(string input)
        {
            string[] parts = input.Split('-');
            int count = parts.Count();
            byte[] result = new byte[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = (byte)int.Parse(parts[i]);
            }
            return result;
        }

        private string ToByteString(byte[] array)
        {
            string result = "-";
            foreach (byte b in array)
                result += b + "-";
            result = result.TrimStart('-').TrimEnd('-');
            return result;
        }

        #endregion

        #endregion
    }
}
