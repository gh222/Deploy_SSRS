﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deploy_SSRS.ReportingServices2010;
using System.IO;
using System.Web.Services.Protocols;
using System.Configuration;

namespace Deploy_SSRS
{
    class SSRS_Deploy
    {
        Deploy_SSRS.ReportingServices2010.ReportingService2010 rs;
        byte[] fileContents;
        string fileNameWithoutExtension;
        string[] List_Of_Reports;
        public string FileSystemPath = @"E:\Reports\";
        public string SSRSWebServiceUrl = @"http://NIBS/ReportServer/ReportService2010.asmx?wsdl";
        public string SSRS_Folder = ConfigurationManager.AppSettings["SSRS_Folder"];
        public string SSRS_DataSource_Name; // = SSRS_Folder + @"_DataSource";
        DataSourceDefinition Definition;
    

        public SSRS_Deploy() {
           SSRS_DataSource_Name = SSRS_Folder + @"_DataSource";
           if(Validate_Parameters())
            {
                Initialize_webservice_proxy();
                CreateSSRS_Folder();
                CreateDataSource();
              //  Upload_Reports();
            
            }
        
        }
        private bool Validate_Parameters() {
            if (string.IsNullOrEmpty(this.FileSystemPath)) { throw new ArgumentNullException("this.FileSystemPath"); }
            //if (!File.Exists(this.FileSystemPath)) { throw new ApplicationException(string.Format("The file [{0}] does not exist", this.FileSystemPath)); }
            if (string.IsNullOrEmpty(this.SSRSWebServiceUrl)) { throw new ArgumentNullException("this.SSRSWebServiceUrl"); }
            if (string.IsNullOrEmpty(this.SSRS_Folder)) { throw new ArgumentNullException("this.SSRS_Folder"); }
            return true;
        }
        private void Initialize_webservice_proxy()
        {
            rs = new Deploy_SSRS.ReportingServices2010.ReportingService2010();
            rs.Url = SSRSWebServiceUrl;
            rs.Credentials = System.Net.CredentialCache.DefaultCredentials;
        }
        private bool CheckIfReportExists(string SsrsItemToFind, string directory,bool DeleteOrNot)
        {
            CatalogItem[] items = null;
            // Retrieve a list of all items from the report server database. 
            try
            {
                items = rs.ListChildren(@"/", true);

                foreach (CatalogItem g in items)
                {
                    if (SsrsItemToFind == g.Name)
                    {
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
        public void CreateSSRS_Folder()
        {
            string directory = @"";
            bool folderExists = CheckIfReportExists(SSRS_Folder, directory,false);
            if (!folderExists)
            {
                rs.CreateFolder(SSRS_Folder, @"/", null);
            }
            Console.WriteLine("Folder Exists => " + SSRS_Folder);
        }
        public void CreateDataSource()
        {
            Definition = new DataSourceDefinition();
            Definition.CredentialRetrieval = CredentialRetrievalEnum.Store;
            Definition.ConnectString = "data source=NIBS;initial catalog=Books";
            Definition.Enabled = true;
            Definition.UserName = "GH";
            Definition.Password = "GH";
            Definition.EnabledSpecified = false;
            Definition.Extension = "SQL";
            Definition.ImpersonateUserSpecified = false;
            Definition.Prompt = null;
            Definition.WindowsCredentials = false;

            Deploy_SSRS.ReportingServices2010.Property[] properties = new Deploy_SSRS.ReportingServices2010.Property[1];
            Deploy_SSRS.ReportingServices2010.Property p = new Deploy_SSRS.ReportingServices2010.Property();
            p.Name = "Hidden"; p.Value = "true";
            properties[0] = p;

            bool IsFolderThere = CheckIfReportExists(SSRS_Folder, "/", false);
            if (IsFolderThere)
            {
                rs.CreateDataSource(SSRS_DataSource_Name, "/" + SSRS_Folder, true, Definition, properties);
                Console.WriteLine("Data Source Created => " + SSRS_DataSource_Name);
            }
            else {
                Console.WriteLine("Data Source NOT Created as no Folder => " + SSRS_Folder);
            }





            string directory = @"";
            bool folderExists = CheckIfReportExists(SSRS_Folder, directory);
            if (!folderExists)
            {
                rs.CreateFolder(SSRS_Folder, @"/", null);
                Console.WriteLine(connection);
            }
            Console.WriteLine("Folder Exists => " + SSRS_Folder);
        }
#endregion

        #region Reports_Upload
        public void Upload_Reports()
        {
            Get_Reports();
            DeleteReport();
            PublishReport();
        }

        private void Get_Reports()
        {
            List_Of_Reports = Directory.GetFiles(FileSystemPath, "*.rdl"); //.Where(item => item.Contains("rdl") == true);
        }
        private void DeleteReport()
        {
            CatalogItem[] items = rs.ListChildren(@"/" + SSRS_Folder, true);
            foreach (CatalogItem g in items)
            {

                if (g.TypeName != "DataSource")
                {
                    rs.DeleteItem(@"/" + SSRS_Folder + @"/" + g.Name);
                    Console.WriteLine(@"Deleted => /" + SSRS_Folder + @"/" + g.Name);
                }

            }

        }

        private void PublishReport()
        {
            DataSource[] sourcs = Report_DataSource();
            Warning[] warnings = null;
            FileInfo fileInfo;
            byte[] fileContents;

            foreach (string s in List_Of_Reports)
            {
                fileInfo = new FileInfo(s);
                fileNameWithoutExtension = Path.GetFileNameWithoutExtension(s);
                fileContents = File.ReadAllBytes(fileInfo.FullName);
                rs.CreateCatalogItem("Report", fileNameWithoutExtension, @"/" + SSRS_Folder, true, fileContents, null, out warnings);
                //   Report_DataSource(@"/" + SSRS_Folder +@"/"+fileNameWithoutExtension);
            }
            if (warnings == null)
            {
                Console.WriteLine("Report Uploaded => " + fileNameWithoutExtension);
            }
            if (warnings != null)
            {
                foreach (Warning warning in warnings)
                {
                    Console.WriteLine(warning);
                }
            }

        }

        private DataSource[] Report_DataSource()
        {
            //DataSourceReference reference = new DataSourceReference();
            DataSource[] sources; = new DataSource[1];
            DataSource s = new DataSource();
            s.Item = Definition;
            s.Name = "Temp_DataSource";
            sources[0] = s;
            //  Console.WriteLine(rdl);
            //rs.SetItemDataSources(rdl, sources);
            return sources;

        }

        #endregion

    }
}
