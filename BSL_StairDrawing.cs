using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Tekla.Structures;
using TSM = Tekla.Structures.Model;
using Tekla.Structures.Model;

namespace Tekla.Technology.Akit.UserScript
{
    public class Script
    {
        public static void Run(Tekla.Technology.Akit.IScript akit)
        {
           string TSBinaryDir = "";
    TSM.Model CurrentModel = new TSM.Model();
    TeklaStructuresSettings.GetAdvancedOption("XSDATADIR", ref TSBinaryDir);

    string ApplicationName = "BSL.StairDrawing.exe";

    // Root extensions folder
    string ExtensionsRoot = Path.Combine(TSBinaryDir, @"Environments\common\extensions");

    string ApplicationPath = null;

    if (Directory.Exists(ExtensionsRoot))
    {
        // Search all subfolders for the exe
        ApplicationPath = Directory
            .GetFiles(ExtensionsRoot, ApplicationName, SearchOption.AllDirectories)
			.ToList()
            .FirstOrDefault();
    }

    if (!string.IsNullOrEmpty(ApplicationPath) && File.Exists(ApplicationPath))
    {
        Process NewProcess = new Process();
        NewProcess.StartInfo.FileName = ApplicationPath;
        NewProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(ApplicationPath);

        try
        {
            NewProcess.Start();
        }
        catch
        {
            MessageBox.Show(ApplicationName + " failed to start.");
        }
    }
    else
    {
        MessageBox.Show(ApplicationName + " not found in extensions folder.");
    }
        }
    }
}