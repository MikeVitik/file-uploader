using System;
using System.IO;
//using System.Net;
//using System.Net.Http;
//using System.Net.Http.Headers;
//using System.Web;
//using System.Web.Http;
//using System.Web.Mvc;
using Microsoft.Net.Http.Headers;


namespace WebService.Helpers
{
    //https://docs.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads
    //https://github.com/aspnet/HttpAbstractions/blob/dev/src/Microsoft.AspNetCore.WebUtilities/MultipartReader.cs
    public static class MultipartRequestHelper
    {
        // Content-Type: multipart/form-data; boundary="----WebKitFormBoundarymx2fSWqWSd0OxQqq"
        // The spec says 70 characters is a reasonable limit.
        public static string GetBoundary(MediaTypeHeaderValue contentType, int lengthLimit)
        {
            var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary);
            if (string.IsNullOrWhiteSpace(boundary.Value))
            {
                throw new InvalidDataException("Missing content-type boundary.");
            }

            if (boundary.Length > lengthLimit)
            {
                throw new InvalidDataException(
                    $"Multipart boundary length limit {lengthLimit} exceeded.");
            }

            return boundary.Value;
        }

        public static bool IsMultipartContentType(string contentType)
        {
            return !string.IsNullOrEmpty(contentType)
                   && contentType.IndexOf("multipart/", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static bool HasFormDataContentDisposition(ContentDispositionHeaderValue contentDisposition)
        {
            // Content-Disposition: form-data; name="key";
            return contentDisposition != null
                   && contentDisposition.DispositionType.Equals("form-data")
                   && string.IsNullOrEmpty(contentDisposition.FileName.Value)
                   && string.IsNullOrEmpty(contentDisposition.FileNameStar.Value);
        }

        public static bool HasFileContentDisposition(ContentDispositionHeaderValue contentDisposition)
        {
            // Content-Disposition: form-data; name="myfile1"; filename="Misc 002.jpg"
            return contentDisposition != null
                   && contentDisposition.DispositionType.Equals("form-data")
                   && (!string.IsNullOrEmpty(contentDisposition.FileName.Value)
                       || !string.IsNullOrEmpty(contentDisposition.FileNameStar.Value));
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