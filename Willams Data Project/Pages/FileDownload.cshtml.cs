using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Willams_Data_Project.Pages
{
    public class DownloadFileModel : PageModel
    {
        public string FileName { get; private set; }
        public string FilePath { get; private set; }

        public IActionResult OnGet(string fileName)
        {
            var rootPath = "./wwwroot/output";
            Console.WriteLine(rootPath);
            FilePath = Path.Combine(rootPath, fileName);

            // Set the file name for download
            FileName = fileName;

            return Page();
        }
    }
}
