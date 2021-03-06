﻿using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
//using System.Net;
//using System.Net.Http;
//using System.Net.Http.Headers;
//using System.Web;
//using System.Web.Http;
//using System.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.ModelBinding;

using WebService.Helpers;

namespace web_service.Controllers
{


    [Route("api/[controller]")]
    public class UploadController : Controller
    {
        [HttpGet]
        public string Get()
        {
            return "test";
        }
        [HttpPost]
        public async void Post()
        {
            FormValueProvider formModel;
            MemoryStream m = new MemoryStream();
            Request.Body.CopyTo(m);
            m.Position = 0;
            var stream = new MemoryStream();//System.IO.File.Create("d:\\temp\\myfile.temp");
            //{
                formModel = await Request.StreamFile(m, stream);
            //}
        }

    }

    //public class UploadController : Controller {
    //    public async Task<IActionResult> Post() {
    //        // Check whether the POST operation is MultiPart?
    //        HttpContentMultipartExtensions.IsMimeMultipartContent()
    //        if(!Request.Content.IsMimeMultipartContent()) {
    //            throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
    //        }

    //        // Prepare CustomMultipartFormDataStreamProvider in which our multipart form
    //        // data will be loaded.
    //        string fileSaveLocation = HttpContext.Server.MapPath("~/App_Data");
    //        CustomMultipartFormDataStreamProvider provider = new CustomMultipartFormDataStreamProvider(fileSaveLocation);
    //        List<string> files = new List<string>();

    //        try {
    //            // Read all contents of multipart message into CustomMultipartFormDataStreamProvider.
    //            await Request.Content.ReadAsMultipartAsync(provider);

    //            foreach(MultipartFileData file in provider.FileData) {
    //                files.Add(Path.GetFileName(file.LocalFileName));
    //            }

    //            // Send OK Response along with saved file names to the client.
    //            return Request.CreateResponse(HttpStatusCode.OK, files);
    //        } catch(System.Exception e) {
    //            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
    //        }
    //    }
    //}

    //// We implement MultipartFormDataStreamProvider to override the filename of File which
    //// will be stored on server, or else the default name will be of the format like Body-
    //// Part_{GUID}. In the following implementation we simply get the FileName from 
    //// ContentDisposition Header of the Request Body.
    //public class CustomMultipartFormDataStreamProvider : MultipartFormDataStreamProvider {
    //    public CustomMultipartFormDataStreamProvider(string path) : base(path) { }

    //    public override string GetLocalFileName(HttpContentHeaders headers) {
    //        return headers.ContentDisposition.FileName.Replace("\"", string.Empty);
    //    }
    //}
}