using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace DemoUploadProgress.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        [HttpPost]
        public async Task<HttpResponseMessage> Upload()
        {
            if (!Request.Content.IsMimeMultipartContent("form-data"))
            {
                return Request.CreateResponse(HttpStatusCode.UnsupportedMediaType);
            }
            var ctx = HttpContext.Current;
            var fileuploadPath = ConfigurationManager.AppSettings["FileUploadLocation"];
            string directory = ctx.Request.Form["directory"] ?? "";
            var root = Path.Combine(ctx.Server.MapPath(fileuploadPath), directory);
            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
            }
            //string fileRemove = ctx.Request.Form["remove"];
            //if (!string.IsNullOrEmpty(fileRemove))
            //{
            // this.Delete(new FileResponse() { url = fileRemove });
            //}
            var provider = new MultipartFormDataStreamProvider(root);
            try
            {
                var filesPosted = new List<FileResponse>();
                await Request.Content.ReadAsMultipartAsync(provider);
                foreach (var file in provider.FileData)
                {
                    string originalFileName = file.Headers.ContentDisposition.FileName.Trim(new Char[] { '"' });
                    if (originalFileName.LastIndexOf("\\") > 0)
                    {
                        originalFileName = originalFileName.Substring(originalFileName.LastIndexOf("\\") + 1);
                    }
                    //var extension = Path.GetExtension(file.Headers.ContentDisposition.FileName.Replace("\"", string.Empty).Replace(@"\", string.Empty));
                    //string originalFileName = String.Format("{0}{1}", DateTime.Now.ToString("yyyyMMddHHmmssfff"), extension);
                    string newFileName = Path.Combine(root, originalFileName);
                    if (File.Exists(newFileName))
                    {
                        File.Delete(newFileName);
                    }
                    File.Move(file.LocalFileName, newFileName);
                    FileInfo fileInfo = new FileInfo(newFileName);
                    //Archivo
                    filesPosted.Add(new FileResponse()
                    {
                        name = originalFileName,
                        size = fileInfo.Length,
                        url = Path.Combine(fileuploadPath.Replace("~", ""), directory, originalFileName).Replace("\\", "/"),
                        isDirectory = false,
                        created = fileInfo.CreationTime
                    });
                    //System.Threading.Thread.Sleep(1);
                }
                return Request.CreateResponse(HttpStatusCode.OK, filesPosted);
            }
            catch (Exception e)
            {
                // Cuando se cancela la operación de subida o se cierra el explorador sin completar la carga
                foreach (var file in provider.FileData)
                {
                    File.Delete(file.LocalFileName);
                }
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
            }
        }
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
