//-----------------------------------------------------------------------
// <copyright file="TwoFAUtility.cs" company="Studio A&T s.r.l.">
//     Copyright (c) Studio A&T s.r.l. All rights reserved.
// </copyright>
// <author>Nicogis</author>
//-----------------------------------------------------------------------
namespace Studioat.ArcGis.Soe.Rest.TwoFAUtility
{
    using System;
    using System.Collections.Specialized;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Web;
    using Base32;
    using ESRI.ArcGIS.Carto;
    using ESRI.ArcGIS.esriSystem;
    using ESRI.ArcGIS.Server;
    using ESRI.ArcGIS.SOESupport;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using OtpSharp;
    using QRCoder;
    using System.Collections.Generic;

    [ComVisible(true)]
    [Guid("de673489-fe8f-4010-91fd-f629972a75c8")]
    [ClassInterface(ClassInterfaceType.None)]
    [ServerObjectExtension("MapServer",
        AllCapabilities = "",
        DefaultCapabilities = "",
        Description = "2FA Utility",
        DisplayName = "2FA Utility",
        Properties = "",
        SupportsREST = true,
        SupportsSOAP = false)]
    public class TwoFAUtility : IServerObjectExtension, IObjectConstruct, IRESTRequestHandler
    {
        // secretKey are from this external file. Advantage of an external file is that 
        // same SOE can be used for multiple services and permission for all of these services
        // is read from the twoFA.json file. 
        private string twoFAFilePath = "C:\\arcgisserver\\twoFA.json";

        private static ReaderWriterLockSlim readWriteLock = new ReaderWriterLockSlim();

        /// <summary>
        /// name of soe
        /// </summary>
        private string soeName;

        /// <summary>
        /// properties of soe
        /// </summary>
        private IPropertySet configProps;

        /// <summary>
        /// object serverObject
        /// </summary>
        private IServerObjectHelper serverObjectHelper;

        // private ServerLogger logger;

        /// <summary>
        /// object rest request Handler
        /// </summary>
        private IRESTRequestHandler reqHandler;

        // virtual folder of service output 
        private string pathOutputVirtualAGS;

        // physical folder of service output   
        private string pathOutputAGS;

        public TwoFAUtility()
        {
            this.soeName = this.GetType().Name;
            // this.logger = new ServerLogger();
            this.reqHandler = new SoeRestImpl(this.soeName, CreateRestSchema()) as IRESTRequestHandler;
        }

        #region IServerObjectExtension Members

        /// <summary>
        /// init() is called once, when the instance of the SOE is created.
        /// </summary>
        /// <param name="pSOH">object server Object</param>
        public void Init(IServerObjectHelper pSOH)
        {
            this.serverObjectHelper = pSOH;
            IMapServer3 mapServer = this.serverObjectHelper.ServerObject as IMapServer3;
            IMapServerInit mapServerInit = mapServer as IMapServerInit;

            this.pathOutputVirtualAGS = mapServerInit.VirtualOutputDirectory;
  
            this.pathOutputAGS = mapServerInit.PhysicalOutputDirectory;

        }

        /// <summary>
        /// shutdown() is called once when the Server Object's context is being shut down and is about to go away.
        /// </summary>
        public void Shutdown()
        {
        }

        #endregion

        #region IObjectConstruct Members

        /// <summary>
        /// construct() is called only once, when the SOE is created, after IServerObjectExtension.init() is called. This
        /// method hands back the configuration properties for the SOE as a property set. You should include any expensive
        /// initialization logic for your SOE within your implementation of construct().
        /// </summary>
        /// <param name="props">object propertySet</param>
        public void Construct(IPropertySet props)
        {
            this.configProps = props;
        }

        #endregion

        #region IRESTRequestHandler Members

        /// <summary>
        /// Get schema 
        /// </summary>
        /// <returns>return schema</returns>
        public string GetSchema()
        {
            return this.reqHandler.GetSchema();
        }

        // <summary>
        /// handle rest request
        /// </summary>
        /// <param name="Capabilities">capabilities of soe</param>
        /// <param name="resourceName">name of resource</param>
        /// <param name="operationName">name of operation</param>
        /// <param name="operationInput">object operationInput</param>
        /// <param name="outputFormat">object outputFormat</param>
        /// <param name="requestProperties">object requestProperties</param>
        /// <param name="responseProperties">object responseProperties</param>
        /// <returns>return handle rest request</returns>
        public byte[] HandleRESTRequest(string Capabilities, string resourceName, string operationName, string operationInput, string outputFormat, string requestProperties, out string responseProperties)
        {
            return this.reqHandler.HandleRESTRequest(Capabilities, resourceName, operationName, operationInput, outputFormat, requestProperties, out responseProperties);
        }

        #endregion

        private RestResource CreateRestSchema()
        {
            RestResource soeResource = new RestResource(this.soeName, false, this.RootResHandler);

            RestResource infoResource = new RestResource("Info", false, this.InfoResHandler);
            soeResource.resources.Add(infoResource);

            RestOperation twoFAOperation = new RestOperation("twoFa",
                                                      new string[] { "issuerID", "reset" },
                                                      new string[] { "json" , "image"},
                                                      this.TwoFAOperationHandler);

            RestOperation addOperation = new RestOperation("addOperation",
                                                      new string[] { "value1", "value2", "code"},
                                                      new string[] { "json" },
                                                      this.AddOperationHandler);

            soeResource.operations.Add(twoFAOperation);
            soeResource.operations.Add(addOperation);

            return soeResource;
        }

        private byte[] RootResHandler(NameValueCollection boundVariables, string outputFormat, string requestProperties, out string responseProperties)
        {
            responseProperties = null;

            JsonObject result = new JsonObject();
            result.AddString("Description", "PoC of 2FA with SOE");

            return result.JsonByte();
        }

        /// <summary>
        /// Returns JSON representation of Info resource. This resource is not a collection.
        /// </summary>
        /// <param name="boundVariables">object boundVariables</param>
        /// <param name="outputFormat">object outputFormat</param>
        /// <param name="requestProperties">object requestProperties</param>
        /// <param name="responseProperties">object responseProperties</param>
        /// <returns>String JSON representation of Info resource.</returns>
        private byte[] InfoResHandler(NameValueCollection boundVariables, string outputFormat, string requestProperties, out string responseProperties)
        {
            responseProperties = "{\"Content-Type\" : \"application/json\"}";

            JsonObject result = new JsonObject();
            AddInPackageAttribute addInPackage = (AddInPackageAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AddInPackageAttribute), false)[0];
            result.AddString("agsVersion", addInPackage.TargetVersion);
            result.AddString("soeVersion", addInPackage.Version);
            result.AddString("author", addInPackage.Author);
            result.AddString("company", addInPackage.Company);

            return result.JsonByte();
        }

        private byte[] TwoFAOperationHandler(NameValueCollection boundVariables,
                                                  JsonObject operationInput,
                                                      string outputFormat,
                                                      string requestProperties,
                                                  out string responseProperties)
        {
            responseProperties = null;

            try
            {
                string userName = (this.GetServerEnvironment() as IServerEnvironment3).UserInfo.Name;

                if (string.IsNullOrWhiteSpace(userName))
                {
                    throw new Exception("user not found!");
                }

                bool reset = false;
                bool? resetValue;
                bool found = operationInput.TryGetAsBoolean("reset", out resetValue);
                
                if (found && resetValue.HasValue)
                {
                    reset = resetValue.HasValue;
                }

                // the issuer ID that will appear on the user's Authenticator app, right above the code. 
                // It should be the name of your app/system so the user can easily identify it.
                string issuerIDValue;
                found = operationInput.TryGetString("issuerID", out issuerIDValue);
                if (!found || string.IsNullOrEmpty(issuerIDValue))
                {
                    throw new ArgumentNullException("issuerID");
                }

                byte[] secretKey = KeyGeneration.GenerateRandomKey(20);

                Users2FA users2FA = null;
                if (File.Exists(this.twoFAFilePath))
                {
                    JObject o = JObject.Parse(File.ReadAllText(this.twoFAFilePath));
                    JsonSerializer serializer = new JsonSerializer();
                    users2FA = (Users2FA)serializer.Deserialize(new JTokenReader(o), typeof(Users2FA));
                    User user = users2FA.users.Where(u => u.name == userName).SingleOrDefault();
                    if (user == null)
                    {
                        users2FA.users.Add(new User() { name = userName, secretKey = Base32Encoder.Encode(secretKey) });
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(user.secretKey) || reset)
                        {
                            user.secretKey = Base32Encoder.Encode(secretKey);
                        }
                        else
                        {
                            secretKey = Base32Encoder.Decode(user.secretKey);
                        }
                    }
                }
                else
                {
                    User user = new User() { name = userName, secretKey = Base32Encoder.Encode(secretKey) };
                    users2FA = new Users2FA() { users = new List<User> { user } };
                }

                string json = JsonConvert.SerializeObject(users2FA, Formatting.Indented);

                readWriteLock.EnterWriteLock();
                try
                {
                    File.WriteAllText(this.twoFAFilePath, json);
                }
                finally
                {
                    // Release lock
                    readWriteLock.ExitWriteLock();
                }

                string url = KeyUrl.GetTotpUrl(secretKey, userName) + $"&issuer={issuerIDValue}";


                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeGenerator.QRCode qrCode = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);

                using (System.Drawing.Bitmap image = qrCode.GetGraphic(20))
                {
                    if (outputFormat == "json")
                    {
                        responseProperties = "{\"Content-Type\" : \"application/json\"}";
                        string fileName = Path.ChangeExtension($"_ags_{Guid.NewGuid().ToString()}", "png");
                        string pathfileName = Path.Combine(this.pathOutputAGS, fileName);
                        image.Save(pathfileName, ImageFormat.Png);
                        JsonObject jsonObject = new JsonObject();
                        jsonObject.AddString("url", $"{this.pathOutputVirtualAGS}/{fileName}");
                        jsonObject.AddLong("status_code", 200);
                        jsonObject.AddString("status_txt", "OK");
                        jsonObject.AddString("text", url);
                        return jsonObject.JsonByte();
                    }
                    else if (outputFormat == "image")
                    {
                        responseProperties = "{\"Content-Type\" : \"image/png\"}";
                        return Helper.ImageToByte(image, ImageFormat.Png);
                    }
                    else
                    {
                        throw new QRCoderException("Format output not found!");
                    }
                }

            }
            catch(Exception ex)
            {
                ObjectError o = new ObjectError(ex.Message);
                return o.ToJsonObject().JsonByte();
            }  
        }

        private byte[] AddOperationHandler(NameValueCollection boundVariables,
                                          JsonObject operationInput,
                                              string outputFormat,
                                              string requestProperties,
                                          out string responseProperties)
        {
            responseProperties = null;

            try
            {
                long? codeValue;
                bool found = operationInput.TryGetAsLong("code", out codeValue);
                if (!found || !codeValue.HasValue)
                {
                    throw new ArgumentNullException("code");
                }

                string userName = (this.GetServerEnvironment() as IServerEnvironment3).UserInfo.Name;

                JObject o = JObject.Parse(File.ReadAllText(this.twoFAFilePath));
                JsonSerializer serializer = new JsonSerializer();
                Users2FA users2FA = (Users2FA)serializer.Deserialize(new JTokenReader(o), typeof(Users2FA));
                User user = users2FA.users.Where(u => u.name == userName).SingleOrDefault();

                byte[] secretKey = null;
                if (user == null)
                {
                    throw new Exception("user not found!");
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(user.secretKey))
                    {
                        throw new Exception("check user configuration!");
                    }
                    else
                    {
                        secretKey = Base32Encoder.Decode(user.secretKey);
                    }
                }

                long timeStepMatched = 0;
                var otp = new Totp(secretKey);

                JsonObject result = new JsonObject();

                // check code from Authenticator App
                if (otp.VerifyTotp(codeValue.ToString(), out timeStepMatched))
                {
                    long? value1Value;
                    found = operationInput.TryGetAsLong("value1", out value1Value);
                    if (!found || !value1Value.HasValue)
                    {
                        throw new ArgumentNullException("value1");
                    }

                    long? value2Value;
                    found = operationInput.TryGetAsLong("value2", out value2Value);
                    if (!found || !value2Value.HasValue)
                    {
                        throw new ArgumentNullException("value2");
                    }

                    result.AddString("result", (value1Value + value2Value).ToString());
                }
                else
                {
                    result.AddString("result", "Code not valid!");
                }


                return Encoding.UTF8.GetBytes(result.ToJson());
            }
            catch(Exception ex)
            {
                ObjectError o = new ObjectError(ex.Message);
                return o.ToJsonObject().JsonByte();
            }
        }

        private IServerEnvironment GetServerEnvironment()
        {
            IEnvironmentManager em = new EnvironmentManagerClass();
            if (em != null)
            {
                UID iseUid = new UIDClass();
                iseUid.Value = "{32d4c328-e473-4615-922c-63c108f55e60}:0";

                try
                {
                    object o = em.GetEnvironment(iseUid);
                    return o as IServerEnvironment;

                }
                catch { }

                return null;
            }

            return null;
        }
    }
}
