using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;

namespace ConsoleApplication1 {
    class Program {
        static void Main(string[] args) {
            using(var client = new HttpClient())
            using(var content = new MultipartFormDataContent()) {
                // Make sure to change API address
                client.BaseAddress = new Uri("http://localhost:5000");

                // Add first file content 
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();

                var list = assembly.GetManifestResourceNames().ToList();//(new String[] { assembly.GetManifestResourceNames().First() }).ToList();
                    list
                    .ForEach(x => {
                        Console.WriteLine(x);
                        var fileContent = new StreamContent(assembly.GetManifestResourceStream(x));
                        fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data") {
                            FileName = x
                        };
                        content.Add(fileContent);
                    });
                    //content.Add(new HttpContent().)

                // Make a call to Web API
                var result = client.PostAsync("/api/upload", content).Result;

                Console.WriteLine(result.StatusCode);
                Console.ReadLine();
            }
        }
    }
}