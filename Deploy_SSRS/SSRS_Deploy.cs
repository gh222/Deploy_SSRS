using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deploy_SSRS.ReportingServices2010;
using System.IO;
using System.Web.Services.Protocols;
using System.Configuration;
using System.Xml;

namespace Deploy_SSRS
{
    class SSRS_Deploy
    {
        Deploy_SSRS.ReportingServices2010.ReportingService2010 rs;
      //  byte[] fileContents;
       // string fileNameWithoutExtension;
      //  string[] List_Of_Reports;
        public string SSRS_Project = ConfigurationManager.AppSettings["SSRS_Project"];
        public string SSRS_Layers = ConfigurationManager.AppSettings["SSRS_Layers"];
        public string SSRS_Path = ConfigurationManager.AppSettings["SSRS_Path"];
        public string SSRS_DataSources = ConfigurationManager.AppSettings["SSRS_DataSources"];
        public string SSRS_WebServiceUrl = ConfigurationManager.AppSettings["SSRS_WebServiceUrl"]; //@"http://NIBS/ReportServer/ReportService2010.asmx?wsdl";
        Dictionary<string, string> All_Reports_Config;
        string[] DataSources_To_Create;

        public SSRS_Deploy() {
            //  string SSRS_DataSource = SSRS_Project + @"_DataSource";
            DataSources_To_Create = SSRS_DataSources.Split('#');
            All_Reports_Config = Get_Reports_From_Config();

            if (Validate_Parameters())
            {
                Initialize_webservice_proxy();
                CreateSSRS_Project_Structure();
                CreateDataSource();
                Upload_Reports();

            }

        }
        private bool Validate_Parameters() {
            if (string.IsNullOrEmpty(this.SSRS_Path)) { throw new ArgumentNullException("this.SSRS_Path"); }
            if (string.IsNullOrEmpty(this.SSRS_WebServiceUrl)) { throw new ArgumentNullException("this.SSRS_WebServiceUrl"); }
            if (string.IsNullOrEmpty(this.SSRS_Project)) { throw new ArgumentNullException("this.SSRS_Project"); }
            if (Validate_DataSource()) { throw new ArgumentNullException("Data Sources Incorrect"); }
     
            return true;
        }
        private bool Validate_DataSource()
        {
            foreach(string s in DataSources_To_Create)
            {
                // Each DataSources_To_Createe must exist in ConnectionStrings
                try { string exists = ConfigurationManager.ConnectionStrings["Connection_" + s].ConnectionString; }
                catch { throw new ArgumentNullException("Each SSRS_DataSources must have a connection string");  }
            }

            foreach (KeyValuePair<string, string> pair in All_Reports_Config)
            {
                // Each Reports Value must exist in SSRS_DataSources
                string[] Report_SSRS_DataSources = pair.Value.Split('#');
                int ipa = 0;
                int cra = 0;
                foreach (string s in Report_SSRS_DataSources)
                {
                    if (s.Substring(0, 3) == "IPA")
                    {
                        ipa++;
                    }
                    if (s.Substring(0, 3) == "CRA")
                    {
                        cra++;
                    }
        
                }
                if(ipa>1 || cra > 1)
                {
                    throw new ArgumentNullException("Layer cant be both IPA/CRA & IPA/CRA_Warehouse");
                }
            }
            return false;
        }
        private void Initialize_webservice_proxy()
        {
            rs = new Deploy_SSRS.ReportingServices2010.ReportingService2010();
            rs.Url = SSRS_WebServiceUrl;
            rs.Credentials = System.Net.CredentialCache.DefaultCredentials;
            
        }
        private bool CheckIfExists(string SsrsItemToFind)
        {
            CatalogItem[] items = null;
            // Retrieve a list of all items from the report server database. 
            try
            {
                items = rs.ListChildren(@"/", true);
                foreach (CatalogItem g in items)
                {
                 //   Console.WriteLine(g.Path);
                    if (SsrsItemToFind == g.Path)
                    {
                        Console.WriteLine("Already Exists => " + SsrsItemToFind);
                        return true;
                    }
                }
            }

            catch (System.Web.Services.Protocols.SoapException e)
            {
                Console.WriteLine(e.Detail.OuterXml);
            }


            return false;
        }

        #region Create_Folder_n_DataSource
        public void CreateSSRS_Project_Structure()
        {
            string[] Layers_To_Create = SSRS_Layers.Split('#');
            var directories = Directory.GetDirectories(SSRS_Path);

            //Create Project Folder
            string folder_To_Create = SSRS_Project;
            string Parent_Folder = @"";
            if (!CheckIfExists(Parent_Folder+"/"+folder_To_Create)) { CreateSSRS_Project(folder_To_Create, Parent_Folder); }
           
            foreach (string layer in Layers_To_Create)
            {
                //Create Layer Folder
                folder_To_Create = layer;
                Parent_Folder = SSRS_Project;
                 if (!CheckIfExists("/" + Parent_Folder + "/" + folder_To_Create)) { CreateSSRS_Project(folder_To_Create, Parent_Folder); }

                foreach (string folder in directories)
                    {
                    //Create Folders
                    folder_To_Create = folder.Substring(folder.LastIndexOf(@"\") + 1, folder.Length - 1 - folder.LastIndexOf(@"\"));
                    Parent_Folder = SSRS_Project+ @"/"+layer;
                    if (!CheckIfExists("/" + Parent_Folder + "/" + folder_To_Create)) { CreateSSRS_Project(folder_To_Create, Parent_Folder); }
                }
            }
            Console.WriteLine(" ");
        }
        public void CreateSSRS_Project(string folder_To_Create, string Parent_Folder)
        {
            //  SSRS_Deploy.ReportService2010.Property[] p = new SSRS_Deploy.ReportService2010.Property[1];
            ReportingServices2010.Property[] properties = new ReportingServices2010.Property[2];
            ReportingServices2010.Property p = new ReportingServices2010.Property();
            p.Name = "Hidden"; p.Value = "false";
            properties[0] = p;
            Property desc = new Property();
            desc.Name = "";
            desc.Value = "";

            if (Parent_Folder == SSRS_Project)
            {
                desc.Name = "Description";

                try
                { string Server = ConfigurationManager.ConnectionStrings["Connection_"+ folder_To_Create].ConnectionString;
                    //  Console.WriteLine( Server.IndexOf("initial", 12) );
                    desc.Value = Server.Substring(12, Server.IndexOf("initial", 12) - 12-1);
                }
                catch { Console.WriteLine("Error In Folder Creation"); }
            }
            properties[1] = desc;
         //   if(folder_To_Create.Contains("IPA") || folder_To_Create.Contains("CRA"))
          //  {
                rs.CreateFolder(folder_To_Create, @"/" + Parent_Folder, properties);
                Console.WriteLine("Create Folder => " + folder_To_Create);
           // }
        }
        public void CreateDataSource()
        {
          //  string[] DataSources_To_Create = SSRS_DataSources.Split('#');
            DataSourceDefinition[] DataSources = new DataSourceDefinition[DataSources_To_Create.Count()];
            DataSourceDefinition Definition;

            int x = 0;
            foreach (string ds in DataSources_To_Create)
            {
                    Definition = new DataSourceDefinition();
                    Definition.CredentialRetrieval = CredentialRetrievalEnum.Store;
                    Definition.ConnectString = ConfigurationManager.ConnectionStrings["Connection_"+ds].ConnectionString; //"data source=NIBS;initial catalog=Books";
                    Definition.Enabled = true;
                    Definition.UserName = ConfigurationManager.AppSettings["SSRS_UserName"];
                    Definition.Password = ConfigurationManager.AppSettings["SSRS_UserPassword"];
                    Definition.EnabledSpecified = false;
                    Definition.Extension = "SQL";
                    Definition.ImpersonateUserSpecified = false;
                    Definition.Prompt = null;
                    Definition.WindowsCredentials = false;
                    DataSources[x] = Definition;
                    ReportingServices2010.Property[] properties = new ReportingServices2010.Property[2];
                    ReportingServices2010.Property p = new ReportingServices2010.Property();
                    p.Name = "Hidden"; p.Value = "true";
                    properties[0] = p;
               
                    Property desc = new Property();
                    desc.Name = "Description";
                    string Server = ConfigurationManager.ConnectionStrings["Connection_" + ds].ConnectionString;
                    desc.Value = Server.Substring(12, Server.IndexOf("initial", 12) - 12 - 1);
                    properties[1] = desc;

                rs.CreateDataSource(@"Data_Source_" + ds, "/" + SSRS_Project, true, Definition, properties);
                    Console.WriteLine("Created DataSource => " + "/" + SSRS_Project + @"/Data_Source_" + ds);
                x++;
           }
            Console.WriteLine("  ");
        }
        #endregion

        #region Reports_Upload
        public void Upload_Reports()
        {
            List<String> All_Reports = Get_Reports_From_Folder(SSRS_Path);
            //All_Reports_Config = Get_Reports_From_Config();
            foreach (string report in All_Reports)
            {
                string[] report_detail = Publish_Report_Get_Config(report);
                if(report_detail[0] == "0")
                {
                    Publish_Report(report_detail);
                }
            }
        }

        private List<String> Get_Reports_From_Folder(string sDir)
        {
            List<String> files = new List<String>();
            try
            {
                foreach (string f in Directory.GetFiles(sDir))
                {
                    files.Add(f);
                }
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    files.AddRange(Get_Reports_From_Folder(d));
                }
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }

            return files;
        }

        private Dictionary<string, string> Get_Reports_From_Config()
        {
            Dictionary<string, string> RDL = new Dictionary<string, string>();
            foreach (string key in ConfigurationManager.AppSettings)
            {
                if (key.Contains("rdl"))
                {
                    string value = ConfigurationManager.AppSettings[key];
                    RDL.Add(key, value);
                }
            }
            return RDL;
        }

        private string[] Publish_Report_Get_Config(string Report_To_Create)
        {
            string RDL_path = Report_To_Create;
            string report_name = Report_To_Create.Substring(Report_To_Create.LastIndexOf(@"\") + 1, -1 + Report_To_Create.Length - Report_To_Create.LastIndexOf(@"\"));
            string folder = Report_To_Create.Replace(report_name, "").Replace(SSRS_Path, "");
            string Layers ="";
           
            if (All_Reports_Config.ContainsKey(report_name))
            {
                Layers =  All_Reports_Config[report_name];
            }
            string all_values = "0";
            if (report_name == null || folder == null || Layers == "") { all_values = "1"; }
                string[] report_detail = new string[] { all_values, report_name, folder, Layers, RDL_path };
            return report_detail;
        }

        private void Publish_Report(string[] report_detail)
        {
            string report_name = report_detail[1];
            string folder = report_detail[2];
            string Layers = report_detail[3];
            string RDL_path = report_detail[4];
            string[] layers = Layers.Split('#');
            foreach (string layer in layers)
            {
                Publish_Report_Create(layer, report_name, folder, RDL_path);
            }
        }

        private void Publish_Report_Create(string layer, string report_name, string folder,string RDL_path)
        {
            Warning[] warnings = null;
            byte[] fileContents = File.ReadAllBytes(RDL_path);
            if (RDL_path.Contains("001"))
            {
                    RDL_path = @"/" + SSRS_Project + "/" + layer + "/" + folder.Replace(@"\", "") + "";
                    rs.CreateCatalogItem("Report", report_name, RDL_path, true, fileContents, null, out warnings);
                    Publish_Report_Set_Reference_DataSource( layer,  report_name,  folder,  RDL_path);
                    Console.WriteLine("created Report => "+ report_name);
            }
        }

        private void Publish_Report_Set_Reference_DataSource(string layer, string report_name, string folder, string RDL_path)
        {
            DataSource[] dsarray;
            string reportName = report_name;// "ACT0001";
            string _selectedFolder = "/"+ SSRS_Project + "/"+ layer + "/"+ folder.Replace(@"\", "") + "/";
            DataSourceReference reference = new DataSourceReference();
            DataSource ds = new DataSource();
            dsarray = new DataSource[1];
            string DSName = "Data_Source_"+ layer;
            reference.Reference = "/"+ SSRS_Project + "/" + DSName;
            ds.Item = reference;
            ds.Name = Publish_Report_Get_Reference_DataSource(RDL_path +"/"+ report_name);
            dsarray[0] = ds;
            rs.SetItemDataSources(_selectedFolder + reportName, dsarray);
        }

        private string Publish_Report_Get_Reference_DataSource(string RDL_path)
        {
            DataSource[] dss =   rs.GetItemDataSources(RDL_path);
            foreach(DataSource d in dss)
            {
                return d.Name.ToString();
            }
            return "None";
        }
           #endregion

        }
}
