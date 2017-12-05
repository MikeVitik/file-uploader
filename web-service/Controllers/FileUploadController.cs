using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
//using System.Net;
//using System.Net.Http;
//using System.Net.Http.Headers;
using System.Threading.Tasks;
//using System.Web;
//using System.Web.Http;
//using System.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.WebUtilities;
using System.Globalization;
using System.Text;


namespace web_service.Controllers { 


public static class MultipartRequestHelper {
        // Content-Type: multipart/form-data; boundary="----WebKitFormBoundarymx2fSWqWSd0OxQqq"
        // The spec says 70 characters is a reasonable limit.
        public static string GetBoundary(MediaTypeHeaderValue contentType, int lengthLimit) {
            var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary);
            if(string.IsNullOrWhiteSpace(boundary.Value)) {
                throw new InvalidDataException("Missing content-type boundary.");
            }

            if(boundary.Length > lengthLimit) {
                throw new InvalidDataException(
                    $"Multipart boundary length limit {lengthLimit} exceeded.");
            }

            return boundary.Value;
        }

        public static bool IsMultipartContentType(string contentType) {
            return !string.IsNullOrEmpty(contentType)
                   && contentType.IndexOf("multipart/", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static bool HasFormDataContentDisposition(ContentDispositionHeaderValue contentDisposition) {
            // Content-Disposition: form-data; name="key";
            return contentDisposition != null
                   && contentDisposition.DispositionType.Equals("form-data")
                   && string.IsNullOrEmpty(contentDisposition.FileName.Value)
                   && string.IsNullOrEmpty(contentDisposition.FileNameStar.Value);
        }

        public static bool HasFileContentDisposition(ContentDispositionHeaderValue contentDisposition) {
            // Content-Disposition: form-data; name="myfile1"; filename="Misc 002.jpg"
            return contentDisposition != null
                   && contentDisposition.DispositionType.Equals("form-data")
                   && (!string.IsNullOrEmpty(contentDisposition.FileName.Value)
                       || !string.IsNullOrEmpty(contentDisposition.FileNameStar.Value));
        }
    }
    public static class FileStreamingHelper {
        private static readonly FormOptions _defaultFormOptions = new FormOptions();

        public static async Task<FormValueProvider> StreamFile(this HttpRequest request, Stream targetStream) {
            if(!MultipartRequestHelper.IsMultipartContentType(request.ContentType)) {
                throw new Exception($"Expected a multipart request, but got {request.ContentType}");
            }

            // Used to accumulate all the form url encoded key value pairs in the 
            // request.
            var formAccumulator = new KeyValueAccumulator();

            var boundary = MultipartRequestHelper.GetBoundary(
                MediaTypeHeaderValue.Parse(request.ContentType),
                _defaultFormOptions.MultipartBoundaryLengthLimit);
            var reader = new MultipartReader(boundary, request.Body);

            var section = await reader.ReadNextSectionAsync();
            while(section != null) {
                ContentDispositionHeaderValue contentDisposition;
                var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out contentDisposition);

                if(hasContentDispositionHeader) {
                    if(MultipartRequestHelper.HasFileContentDisposition(contentDisposition)) {
                        await section.Body.CopyToAsync(targetStream);
                    } else if(MultipartRequestHelper.HasFormDataContentDisposition(contentDisposition)) {
                        // Content-Disposition: form-data; name="key"
                        //
                        // value

                        // Do not limit the key name length here because the 
                        // multipart headers length limit is already in effect.
                        var key = HeaderUtilities.RemoveQuotes(contentDisposition.Name);
                        var encoding = GetEncoding(section);
                        using(var streamReader = new StreamReader(
                            section.Body,
                            encoding,
                            detectEncodingFromByteOrderMarks: true,
                            bufferSize: 1024,
                            leaveOpen: true)) {
                            // The value length limit is enforced by MultipartBodyLengthLimit
                            var value = await streamReader.ReadToEndAsync();
                            if(String.Equals(value, "undefined", StringComparison.OrdinalIgnoreCase)) {
                                value = String.Empty;
                            }
                            formAccumulator.Append(key.Value, value);

                            if(formAccumulator.ValueCount > _defaultFormOptions.ValueCountLimit) {
                                throw new InvalidDataException($"Form key count limit {_defaultFormOptions.ValueCountLimit} exceeded.");
                            }
                        }
                    }
                }

                // Drains any remaining section body that has not been consumed and
                // reads the headers for the next section.
                section = await reader.ReadNextSectionAsync();
            }

            // Bind form data to a model
            var formValueProvider = new FormValueProvider(
                BindingSource.Form,
                new FormCollection(formAccumulator.GetResults()),
                CultureInfo.CurrentCulture);

            return formValueProvider;
        }

        private static Encoding GetEncoding(MultipartSection section) {
            MediaTypeHeaderValue mediaType;
            var hasMediaTypeHeader = MediaTypeHeaderValue.TryParse(section.ContentType, out mediaType);
            // UTF-7 is insecure and should not be honored. UTF-8 will succeed in 
            // most cases.
            if(!hasMediaTypeHeader || Encoding.UTF7.Equals(mediaType.Encoding)) {
                return Encoding.UTF8;
            }
            return mediaType.Encoding;
        }
    }


    [Route("api/[controller]")]
    public class UploadController : Controller {
        [HttpGet]
        public string Get() {
            return "test";
        }
        [HttpPost]
        public async void Post() {
            FormValueProvider formModel;
            using(var stream = System.IO.File.Create("d:\\temp\\myfile.temp")) {
                formModel = await Request.StreamFile(stream);
            }
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