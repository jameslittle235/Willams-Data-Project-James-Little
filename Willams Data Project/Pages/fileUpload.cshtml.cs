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
using System.Threading.Channels;

namespace Willams_Data_Project.Pages
{
    public class FileMonitorModel : PageModel
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        public List<string> channelNames { get; set; } = new List<string>();
        public List<string> FileNames { get; set; } = new List<string>();
        public string SelectedFileName { get; set; }
        public List<string> LoadedFiles { get; set; } = new List<string>();
        public List<line> channel1 = new List<line>();
        public List<line> channel2 = new List<line>();
        public List<line> channel3 = new List<line>();
        public List<line> dataObjects = new List<line>();
        public FileMonitorModel(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public void OnGet()
        {
            // Specify the folder path to monitor
            string folderPath = @"./DataMonitor"; // Change this to your folder path

            // Get existing files in the folder
            FileNames.AddRange(Directory.GetFiles(folderPath));

            // Start monitoring the folder for new files
            var fileWatcher = new FileSystemWatcher(folderPath);
            //fileWatcher.Created += OnFileCreated;
            fileWatcher.EnableRaisingEvents = true;
        }

        public IActionResult OnPostSelectFile(string filename)
        {
            Console.WriteLine(filename);
            if (filename != null)
            {
                dataObjects = ReadDataFile(filename); //calls ReadDataFile procedure and returns result to list of objects
                Console.WriteLine(dataObjects);
                var channels = dataObjects  //create a list of lines for each channel that dynamically scales
                    .GroupBy(p => p.Channel)
                    .Select(o => o.First());
                foreach(var channel in channels) //populate channelNames list
                {
                    channelNames.Add(channel.Channel);
                }
                    

                return Page();


            }
            return Page();
        }
        static List<line> ReadDataFile(string filePath)
        {
            List<line> dataObjects = new List<line>();

            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    bool isFirstLine = true; // Flag to skip the first line

                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();

                        if (isFirstLine)
                        {
                            isFirstLine = false;
                            continue; // Skip the first line
                        }

                        string[] fields = line.Split('|');

                        if (fields.Length == 3)
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
        public IActionResult OnPostAddChannelFilters()
        {
            Console.WriteLine(Request.Form["channelName"]);
            Console.WriteLine(Request.Form["channelFilter"]);


            foreach (var obj in dataObjects) //prints each object out from the list of objects
            {
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
                if (obj.Channel == "channel 2" && obj.Value == 2)
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
            string channel1String = JsonSerializer.Serialize(channel1);
            string channel2String = JsonSerializer.Serialize(channel2);
            string channel3String = JsonSerializer.Serialize(channel3);
            System.IO.File.WriteAllText("./wwwroot/output/after_output.json", channel1String + channel2String + channel3String);
            string beforeOutputString = JsonSerializer.Serialize(dataObjects);
            System.IO.File.WriteAllText("./wwwroot/output/before_output.json", beforeOutputString);

            LoadedFiles.Clear();
            LoadedFiles.Add("before_output.json");
            LoadedFiles.Add("after_output.json");


            return  Page();
        }
        public class line
        {
            public string Timestamp { get; set; }
            public float Value { get; set; }
            public string Channel { get; set; }
        }


    }
    

        }

