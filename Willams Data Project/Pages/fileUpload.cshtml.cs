using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Hosting;

namespace Willams_Data_Project.Pages
{
    public class FileMonitorModel : PageModel
    {
        private readonly IWebHostEnvironment _hostingEnvironment;

        public List<string> FileNames { get; set; } = new List<string>(); //stores the list of file names in the monitored folder
        public List<string> LoadedFiles { get; set; } = new List<string>(); //stores the list of file names for transformed data to download
        public string SelectedFileName { get; set; }
        public FileMonitorModel(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public void OnGet()
        {
            string folderPath = @"./DataMonitor";  // folder to monitor


            // Get existing files in the folder
            FileNames.AddRange(Directory.GetFiles(folderPath));

            // Start monitoring the folder for new files
            var fileWatcher = new FileSystemWatcher(folderPath);
            fileWatcher.EnableRaisingEvents = true;
        }

        public IActionResult OnPost(string filename)
        {

            if (filename != null)
            {
                // Process the chosen file and get two new file names
                List<line> dataObjects = ReadDataFile(filename); //calls ReadDataFile procedure and returns result to list of objects
                List<line> channel1 = new List<line>();
                List<line> channel2 = new List<line>();
                List<line> channel3 = new List<line>();

                foreach (var obj in dataObjects) //prints each object out from the list of objects
                {
                    //runs hardcoded filters on the data 
                    if (obj.Channel == "channel 1" && obj.Value == 2)
                    {
                        line channel1obj = new line
                        {
                            Timestamp = obj.Timestamp,
                            Value = obj.Value,
                            Channel = obj.Channel
                        };
                        channel1.Add(channel1obj);
                    }
                    if (obj.Channel == "channel 3" && obj.Value < 3)
                    {
                        line channel3obj = new line
                        {
                            Timestamp = obj.Timestamp,
                            Value = obj.Value,
                            Channel = obj.Channel
                        };
                        channel3.Add(channel3obj);
                    }
                }
                //converts the object lists of data matching the filter into jsons for exporting
                    string channel1String = JsonSerializer.Serialize(channel1);
                    string channel3String = JsonSerializer.Serialize(channel3);
                //saves file using json data
                    System.IO.File.WriteAllText("./wwwroot/output/after_output.json", channel1String + channel3String);
                //creates a json of all the data from the binary file
                    string beforeOutputString = JsonSerializer.Serialize(dataObjects);
                //saves all binary data to a json file
                    System.IO.File.WriteAllText("./wwwroot/output/before_output.json", beforeOutputString);

                //adds the file names to the list of files
                    LoadedFiles.Clear();
                    LoadedFiles.Add("before_output.json");
                    LoadedFiles.Add("after_output.json");
                

                return Page();


            }
            return null;
        }
        static List<line> ReadDataFile(string filePath)
        {
            List<line> dataObjects = new List<line>(); //create a list of lines to effectively store the file in a readable format

            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    bool isFirstLine = true; // Flag to skip the first line,
                                             // this means it skips over the header fields which needs to be factored in as it might not always be needed

                    while (!reader.EndOfStream) //while theres still data to read from the file
                    {
                        string line = reader.ReadLine(); //store one line

                        if (isFirstLine)
                        {
                            isFirstLine = false;
                            continue; // Skip the first line
                        }

                        string[] fields = line.Split('|'); //split the fields

                        if (fields.Length == 3) //ensure we have the three fields we expected
                        {
                            line obj = new line
                            {
                                Timestamp = fields[0],
                                Value = float.Parse(fields[1]),
                                Channel = fields[2]
                            };

                            dataObjects.Add(obj);
                        }
                        else
                        {
                            // Handle invalid lines or log an error.
                            Console.WriteLine($"Skipping invalid line: {line}");
                        }
                    }
                }
            }
            catch (IOException e)
            {
                Console.WriteLine($"Error reading the file: {e.Message}");
            }

            return dataObjects;
        }
        public class line
        {
            public string Timestamp { get; set; }
            public float Value { get; set; }
            public string Channel { get; set; }
        }
    }

}