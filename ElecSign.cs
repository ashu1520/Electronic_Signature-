
using DocuSign.eSign.Api;
using DocuSign.eSign.Client;
using DocuSign.eSign.Client.Auth;
using DocuSign.eSign.Model;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Octokit;
using System;
using System.Configuration;


namespace DocuSign
{
    class ElecSign
    {
        static string integrationKey;
        static string userId;
        static string accountId;
        static string authserver;
        static string recipientName;
        static string recipientEmail;
        static string rsaKeyPair;
        static string TestFileName;
        static int sleepDelay;
        static int NoOfReq;
        static List<string> scopes = new List<string>
        {
            "signature"
        };

        static void Main(string[] args)
        {
            var apiClinet = new eSign.Client.ApiClient("https://demo.docusign.net/restapi");
            integrationKey = ConfigurationManager.AppSettings["INTEGRATIONKEY"];
            userId = ConfigurationManager.AppSettings["USERID"];
            accountId = ConfigurationManager.AppSettings["ACCOUNTID"];
            authserver = ConfigurationManager.AppSettings["AUTHSERVER"];
            rsaKeyPair = ConfigurationManager.AppSettings["RSAKEYPAIR"];
            recipientName = ConfigurationManager.AppSettings["USERNAME"];
            recipientEmail = ConfigurationManager.AppSettings["USEREMAIL"];
            sleepDelay = Convert.ToInt16("0" + ConfigurationManager.AppSettings["DELAYTIMER"]);
            TestFileName = ConfigurationManager.AppSettings["TESTFILE"];
            NoOfReq = Convert.ToInt16("0" + ConfigurationManager.AppSettings["REQUESTCOUNT"]);
            string[] repositoryUrls = ConfigurationManager.AppSettings.AllKeys;

            //Validation for all keys
            foreach(var value in repositoryUrls)
            {
                if (string.IsNullOrEmpty(ConfigurationManager.AppSettings[value]))
                {
                    Console.WriteLine(value + "Can not be null. Please check into the App.Config file.");
                    Console.Read();
                    return;
                }
            }

            //Console log
            Console.WriteLine("Sending Electronic Signature Request for below details:-- \n");
            Console.WriteLine("Integration key/Api Key: {0}\n",integrationKey);
            Console.WriteLine("User Id:{0}\n", userId);
            Console.WriteLine("Account Id:{0}\n", accountId);
            Console.WriteLine("AuthServer :{0}\n", authserver);
            Console.WriteLine("Recipientname : {0}\n", recipientName);
            Console.WriteLine("RecipientEmail :{0}\n", recipientEmail);
            Console.WriteLine("Sleep Delay :{0}\n", sleepDelay);
            Console.WriteLine("Number of Request :{0}\n", NoOfReq);

            Console.WriteLine("Test File Name: {0} \n", TestFileName);
            Console.WriteLine("**********************************************************************");

            if(sleepDelay == 0)
            {
                sleepDelay = 5000;//5 Sec
            }
            else
            {
                sleepDelay = sleepDelay * 60 * 1000;
            }

            for(int i=0;i < NoOfReq; i++)
            {
                ElecSign recipes = new ElecSign();

                //Acceess Token
                OAuth.OAuthToken authToken = apiClinet.RequestJWTUserToken(integrationKey, userId, authserver, Encoding.UTF8.GetBytes(rsaKeyPair), 1, scopes);
                string accessToken = authToken.access_token;

                EnvelopesApi envelopesApi = new EnvelopesApi(apiClinet);
                byte[] fileBytes = File.ReadAllBytes(TestFileName);

                EnvelopeSummary envelopeSummary = envelopesApi.CreateEnvelope(accountId, envelopeDefinition());
                Console.WriteLine("Envelope: \n{0}", JsonConvert.SerializeObject(envelopeSummary));
                System.Threading.Thread.Sleep(sleepDelay);

            }
            Console.WriteLine("Completed Successfull!! Press Enter to close!!");
            Console.Read();
        }

        private static EnvelopeDefinition envelopeDefinition()
        {
            byte[] fileBytes = File.ReadAllBytes(TestFileName);
            EnvelopeDefinition envelopeDef = new EnvelopeDefinition();
            envelopeDef.EmailSubject = ConfigurationManager.AppSettings["DOCSUBJECT"];

            //aDD DOC
            Document doc = new Document();
            doc.DocumentBase64 = System.Convert.ToBase64String(fileBytes);
            doc.Name = ConfigurationManager.AppSettings["DOCNAME"];
            doc.DocumentId = "1";
            envelopeDef.Documents = new List<Document>();
            envelopeDef.Documents.Add(doc);

            //Add Receipent

            Signer signer = new Signer();
            signer.Email = ConfigurationManager.AppSettings["USEREMAIL"];
            signer.Name = ConfigurationManager.AppSettings["USERNAME"];
            signer.RecipientId = "1";

            //Create Signer tab in Doc.
            signer.Tabs = new Tabs();
            signer.Tabs.SignHereTabs = new List<SignHere>();
            SignHere signHere = new SignHere();
            signHere.DocumentId = "1";
            signHere.PageNumber = "1";
            signHere.RecipientId = "1";
            signHere.XPosition = "100";
            signHere.YPosition = "100";
            signer.Tabs.SignHereTabs.Add(signHere);

            envelopeDef.Recipients = new Recipients();
            envelopeDef.Recipients.Signers = new List<Signer>();
            envelopeDef.Recipients.Signers.Add(signer);

            //set envelope status to "sent" to immediately 
            envelopeDef.Status = "sent";

            return envelopeDef;
        }
    }
}