using Dapper;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Seq.App.Asana;
using TaskManagement.Domain;
using TaskManagement.Persistance;
using TaskManagement.Persistance.Implementation;
using System.Configuration;
using TaskManagement.Models;
using Amazon.S3;
using Newtonsoft.Json.Linq;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Amazon.S3.Transfer;
using System.IO;
using TaskManagement.Filters;
using Dapper;

namespace TaskManagement.Controllers
{
    [AuthenticationFilter]
    public class DocumentController : Controller
    {
        // GET: Document
        DocumentDao doctDto = new DocumentDao();
        public DocumentController()
        {
            string name = Environment.UserName;//get the name of the logged user in system
            ViewBag.user = name;
            // using this data to populate project id list data
            List<project> projctList = doctDto.GetProject();
            ViewBag.projects = projctList;
        }

        public ActionResult DocumentIndex()
        {
            int StartRow = 1, page = 1, pageSize = 20, EndRow = 1;
            page = Request.Form["page"] != null ? Convert.ToInt16(Request.Form["page"]) : 1;
            StartRow = ((page - 1) * (pageSize))+1;
            EndRow = (StartRow-1)  + pageSize;

            Hashtable hashtable = new Hashtable();

            ViewBag.user = Environment.UserName;
            List<Document> documents = doctDto.SearchforDocuments(StartRow, EndRow,GenerateParam(hashtable));
            ViewBag.Document = documents;
           // ViewBag.Document = doctDto.GetDocument();

            ViewBag.pagesize = pageSize;
            ViewBag.CurrentPage = page;
            if (documents != null && documents.Count > 0)
            {
                ViewBag.TotalCount = documents[0].TotalCount;
            }
            return View();
        }


        public ActionResult AddDocument(){
            ViewBag.user = Environment.UserName;
            List<string> projList = new List<string>();
            string ProjectName = Request.Form["ProjectName"] != null ? Request.Form["ProjectName"] : string.Empty;
            projList.Add(ProjectName);
            string MainSection = Request.Form["MainSection"] != null ? Request.Form["MainSection"] : string.Empty;
            projList.Add(MainSection);
            string Controller = Request.Form["Controller"] != null ? Request.Form["Controller"] : string.Empty;
            projList.Add(Controller);
            string Action = Request.Form["Action"] != null ? Request.Form["Action"] : string.Empty;
            projList.Add(Action);
            string Url = Request.Form["Url"] != null ? Request.Form["Url"] : string.Empty;
            projList.Add(Url);
            string SubSection = Request.Form["SubSection"] != null ? Request.Form["SubSection"] : string.Empty;
            projList.Add( SubSection);
            string FunctionalityDescription = Request.Form["FunctionalityDescription"] != null ? Request.Form["FunctionalityDescription"] : string.Empty;
            projList.Add(FunctionalityDescription);
            string ProjectId=Request.Form["ProjectId"] != null ? Request.Form["ProjectId"] : string.Empty;
            projList.Add(ProjectId);
            ViewBag.projectData = projList;
            return View();
        }

        public ActionResult EditDocument(int docid,string from = "")
        {
            Document document = doctDto.GetDocumentDetails(docid);
            ViewBag.Document = document;
            ViewBag.From = from;
            //return RedirectToAction("Document", "AddDocument", new { from = "analysis" });
            return View("AddDocument",document);
        }

        public ActionResult SearchDocument(Document document)
        {
            int StartRow = 1, page = 1, pageSize = 20, EndRow = 1;

            page = Request.Form["page"] != null ? Convert.ToInt16(Request.Form["page"]) : Request.QueryString["page"] != null ? Convert.ToInt16(Request.QueryString["page"]) : 1;
            StartRow = ((page - 1) * (pageSize)) + 1;
            EndRow = StartRow-1 + pageSize;


            Hashtable hashtable = new Hashtable();
            
            hashtable.Add("type", "documentsearch");
            hashtable.Add("projectid", document.ProjectId);
            hashtable.Add("mainsection", document.MainSection);
            hashtable.Add("controller", document.ControllerName);
            hashtable.Add("action", document.ActionName);
            hashtable.Add("url", document.Url);

            List<Document> documents = doctDto.SearchforDocuments(StartRow, EndRow, GenerateParam(hashtable));

            ViewBag.Document = documents;

            ViewBag.pagesize = pageSize;
            ViewBag.CurrentPage = page;
            if (documents != null && documents.Count > 0)
            {
                ViewBag.TotalCount = documents[0].TotalCount;
            }
            return View();
        }

        public string GenerateParam(Hashtable ht)
        {
            string param = "";

            param = param + "<params>";

            if (ht !=null && ht["type"] !=null && ht["type"].ToString() == "documentsearch")
            {
                param += "<param name=\"ProjectId\"><value val=\"" + ht["projectid"] + "\"></value></param>"
                      + "<param name=\"Mainsection\"><value val=\"" + ht["mainsection"] + "\"></value></param>"
                      + "<param name=\"Controller\"><value val=\"" + ht["controller"] + "\"></value></param>"
                      + "<param name=\"Action\"><value val=\"" + ht["action"] + "\"></value></param>"
                      + "<param name=\"Url\"><value val=\"" + ht["url"] + "\"></value></param>";
            }
            param = param + "</params>";

            return param;
        }

        //form the sumitted data as xml and send param to stored procedure
        [HttpPost]
        [AllowAnonymous]
        public ActionResult saveDocument(Document document) {
          //  if(model.ProjectId=="" || model.MainSection.)
            //    return Redirect("../Document/AddDocument");
            string result = "";
            StringBuilder client = new StringBuilder();                    //client stringBulder is used for holding <ClientSide Type=""> data like wise  server,database,service,
            StringBuilder server = new StringBuilder();
            StringBuilder database = new StringBuilder();
            StringBuilder service = new StringBuilder();
            StringBuilder param = new StringBuilder();                   //param is used for holding client,server, database, service data's
            string commonRow = Request.Form["common_row"]; //used for store added client,server.database, service data's ex: it store client_row0,server_row0,database_row0,service_row0
            string[] commonArray = commonRow.Split(',');
            string clients, servers, databases, services;
            document.created_by =Request.Form["CreatedBy"];
            string created_by = document.created_by;
            int document_id =0;
            document.docFunList = new List<DocumentFunction>();
            DocumentDao docDao = new DocumentDao();
            try {

                if (document.Image != null)
                {
                    List<project> projctList = doctDto.GetProject();
                    string projectname = projctList.Where(w => w.Id == document.ProjectId).Select(s => s.ProjectName).First();
                    string AWSAccessKey = ConfigurationManager.AppSettings["AWSAccessKey"].ToString();
                    string AWSSecretKey = ConfigurationManager.AppSettings["AWSSecretKey"].ToString();
                    var s3client = new AmazonS3Client(AWSAccessKey, AWSSecretKey, Amazon.RegionEndpoint.USEast1);
                    TransferUtility utility = new TransferUtility(s3client);
                    TransferUtilityUploadRequest request = new TransferUtilityUploadRequest();
                    Stream inputstream = document.Image.InputStream != null ? document.Image.InputStream : null;
                    string contenttype = "image/jpeg";
                    string FileName = document.MainSection;
                    string Extension = document.Image.FileName.Split('.')[1];
                    string bucket = !string.IsNullOrEmpty(ConfigurationManager.AppSettings["imagebucket"]) ? ConfigurationManager.AppSettings["imagebucket"] : string.Empty;
                   
                    FileName = FileName + "." + Extension;
                    request.BucketName = bucket+projectname;
                    request.CannedACL = S3CannedACL.PublicRead;
                    request.Key = FileName;
                    request.InputStream = inputstream;
                    request.ContentType = contenttype;
                    utility.Upload(request);
                    Amazon.S3.IO.S3FileInfo fileinfo = new Amazon.S3.IO.S3FileInfo(s3client, bucket+projectname, Path.GetFileName(FileName));
                    if (fileinfo.Exists)
                    {
                        document.ImageName = FileName;
                        var res = "success";
                    }
                }

                string root = "<Root>"
               + "<Document  ProjectId=\"" + document.ProjectId + "\" MainSection=\"" + document.MainSection + "\"  Controller=\"" + document.Controller + "\" Action=\"" + document.Action + "\" Url=\"" + document.Url + "\" SubSection=\"" + document.SubSection + "\" FunctionalityDescription=\"" + document.FunctionalityDescription + "\" Image=\"" + document.ImageName + "\"    >";
                clients = "<ClientSide Type=\"" + "Client" + "\">";
                servers = "<ServerSide Type =\"" + "server" + "\">";
                databases = "<Database  Type=\"" + "Database" + "\">";
                services = "<Service  Type=\"" + "service" + "\"> ";
                param.Append(root);
                client.Append(clients);
                server.Append(servers);
                database.Append(databases);
                service.Append(services);
               foreach (string str in commonArray)    
               {
                    if (string.IsNullOrEmpty(str))
                        continue;
                    DocumentFunction docFun = new DocumentFunction();
                    docFun.FunctionName = Request.Form[str + "Name"];
                    if (!string.IsNullOrEmpty(docFun.FunctionName))
                    {
                        docFun.Input = Request.Form[str + "input"];
                        docFun.Output = Request.Form[str + "output"];
                        docFun.FunctionDescription = Request.Form[str + "dDescription"];

                        docFun.ServerSideScripting = Request.Form[str + "server"];
                        docFun.Database = Request.Form[str + "database"];
                        docFun.Service = Request.Form[str + "service"];
                        docFun.LinkingMethod = Request.Form[str + "linkmethod"];

                        docFun.Procedure = Request.Form[str + "procedure"];
                        docFun.TableorViewUsed = Request.Form[str + "tables"];

                        docFun.CreatedBy = Request.Form["CreatedBy"];
                        docFun.Type = str.Substring(0, str.IndexOf('_'));  //get the function type whether it is client or server or database or service 
                        document.docFunList.Add(docFun);
                        string source = "<Source  FunctionName=\"" + docFun.FunctionName + "\" FunctionDescription =\"" + docFun.FunctionDescription + "\" Input=\"" + docFun.Input + "\"" + " Output =\"" + docFun.Output + "\" ServerSideScripting =\"" + docFun.ServerSideScripting + "\" Database =\"" + docFun.Database + "\" Service =\"" + docFun.Service + "\" LinkingMethod =\"" + docFun.LinkingMethod + "\" Procedure =\"" + docFun.Procedure + "\" TableorViewUsed =\"" + docFun.TableorViewUsed + "\"  />";
                        if (docFun.Type.ToString() == "client")
                            client.Append(source);
                        else if (docFun.Type.ToString() == "server")
                            server.Append(source);
                        else if (docFun.Type.ToString() == "database")
                            database.Append(source);
                        else if (docFun.Type.ToString() == "service")
                            service.Append(source);
                    }
                }
                client.Append("</ClientSide>");
                server.Append("</ServerSide>");
                database.Append("</Database>");
                service.Append("</Service>");
                param.Append(client);
                param.Append(server);
                param.Append(database);
                param.Append(service);
                param.Append("</Document></Root>");
                result = docDao.InsertNewDocument(param, document.Id, created_by);
            }
            catch (Exception e)
            {

            }
            //if (result == "success")
            //    return "success";
            //else
            //    return "failed";
            if(document !=null && document.Id != 0)
            {
                return RedirectToAction("EditDocument", "Document", new { docid = document.Id, from = "update"});
            }
            else
            {
                return RedirectToAction("AddDocument","Document");
            }

        }
        public ActionResult AssignAsanaTask()
        {

            return View();
        }

        [HttpPost]
        public ActionResult AssignAsanaTask(List<string> id, List<string> time)
        {

            Authentication authentication = new Authentication("0/d446566a54d8e978c881e856b12ae028");
            Dictionary<string, string> ids = new Dictionary<string, string>();
            try
            {
                for (int i = 0; i < id.Count; i++)
                {
                    if (id[i] != string.Empty)
                        ids.Add(id[i], time[i]);
                }
            }
            catch (Exception e)
            {
                ViewBag.error = e.Message;
            }
            //  var sut = AsanaTask.Retreive<AsanaTask>("830190616500438", authentication);
            foreach (string asanaid in ids.Keys)
            {
                var sut = AsanaTask.Retreive<AsanaTask>(asanaid, authentication);
                //var sut = AsanaTask.Retreive<AsanaTask>("819844734307059", authentication);
                if (sut == "404")
                {
                    ViewBag.task = asanaid;
                }
                else if (sut != string.Empty)
                {
                    JObject jObject = JObject.Parse(sut.ToString());
                    List<Datum> tag = new List<Datum>();
                    //tag= jObject["data"].Select(j => j["assignee"].ToObject<Datum>()).ToList();
                    //tag = jObject["data"].Select(j => j["assignee_status"].ToObject<Datum>()).ToList();
                    //tag = jObject["data"].Select(j => j["completed_at"].ToObject<Datum>()).ToList();
                    //tag = jObject["data"].Select(j => j["created_at"].ToObject<Datum>()).ToList();
                    tag = jObject["data"]["assignee"].Select(j => j.ToObject<Datum>()).ToList();
                    int x1 = 0;
                }

            }



            //var sut = AsanaTask.Retreive<AsanaTask>("205337626400920", authentication);

            int x = 0;
            return View();
        }



        // get the function name data and form the data in javascript object
        public StringBuilder getData(string FunctionName, int ProjectId, string Type) {

            try {
                DocumentFunction docFun = doctDto.GetFunctionDetail(FunctionName, ProjectId, Type);
              //if (docFun.Input == null || docFun.Output == null || docFun.FunctionDescription == null)
                //    return null;
                string[] response = new string[3];
                response[0] = docFun.Input.Trim();             //remove leading and tailing white spaces in get data
                response[1] = docFun.Output.Trim();
                response[2] = docFun.FunctionDescription.Trim();
                StringBuilder sbuild = new StringBuilder();
                sbuild.Append("{input:\"");
                sbuild.Append((!string.IsNullOrEmpty(docFun.Input) ? docFun.Input.Trim() : "") + "\",");
                sbuild.Append("output: \"");
                sbuild.Append((!string.IsNullOrEmpty(docFun.Output) ? docFun.Output.Trim() : "") + "\",");
                sbuild.Append("serversidescripting: \"");
                sbuild.Append((!string.IsNullOrEmpty(docFun.ServerSideScripting) ? docFun.ServerSideScripting.Trim() : "") + "\",");
                sbuild.Append("database: \"");
                sbuild.Append((!string.IsNullOrEmpty(docFun.Database) ? docFun.Database.Trim() : "") + "\",");
                sbuild.Append("service: \"");
                sbuild.Append((!string.IsNullOrEmpty(docFun.Service) ? docFun.Service.Trim() : "") + "\",");
                sbuild.Append("linkingmethod: \"");
                sbuild.Append((!string.IsNullOrEmpty(docFun.LinkingMethod) ? docFun.LinkingMethod.Trim() : "") + "\",");
                sbuild.Append("procedure: \"");
                sbuild.Append((!string.IsNullOrEmpty(docFun.Procedure) ? docFun.Procedure.Trim() : "") + "\",");
                sbuild.Append("tableorview: \"");
                sbuild.Append((!string.IsNullOrEmpty(docFun.TableorViewUsed) ? docFun.TableorViewUsed.Trim() : "") + "\",");
                sbuild.Append("functionDescription: \"");
                sbuild.Append((!string.IsNullOrEmpty(docFun.FunctionDescription) ? docFun.FunctionDescription.Trim() : "") + "\" }");

               return sbuild;
                 }
              catch(Exception ex)
              {
                return null;
              }

          }
        }
     }
