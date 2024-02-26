using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace UniversalLauncher
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // Application.Run(new MainForm());
            // LauncherProgram new LauncherProgram(args);
            // MessageBox.Show("Enterring program...", "Debug", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            String ArgumentReceived = string.Join(" ", args);
            ProgramConfig Config = new ProgramConfig(ArgumentReceived);
            if (Config.Target.Exist != true) 
            {
                // the target does not exist the program should show an error except if HideNotExistError
                if (Config.HideNotExistError == false) {
                    MessageBox.Show(Config.Target.ToLaunch, "File not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return;
            }
            Config.Execute();
            //MessageBox.Show("Exitting program...", "Debug", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
    }
    public enum EnvVarAction
    {
        Create = 0,
        Append = 1,
        Prepend = 2,
    }
    public enum ProgramType 
    {
        Executable = 0,
        Command = 1,
        VBScript = 2,
        PowershellScript = 3,
        Unknown = 4
    }
    public class ProgramConfig {
        public ProgramInfo Target;
        public String ProcessToRun;
        public String ProcessArguments;
        public Boolean HideNotExistError = false;
        public Boolean Wait;
        public Boolean Verbose;
        public Boolean HideWindow;
        public String OriginalArguments;
        public String AppendArguments;
        public String PrependArguments;
        public String FinalArguments;
        public List<EnvVar> Variables = new List<EnvVar>();
        public Boolean ShouldLaunch = true;
        public ProgramConfig(string arguments)
        {
            OriginalArguments = arguments;
            String ToLaunch = ReadConfig("Path", "");
            String WorkingDir = ReadConfig("WorkingDir", "");
            Target = new ProgramInfo(ToLaunch,WorkingDir);
            Wait = Convert.ToBoolean(ReadConfig("Wait", "False"));
            HideNotExistError = Convert.ToBoolean(ReadConfig("HideNotExistError", "False"));
            Verbose = Convert.ToBoolean(ReadConfig("Verbose", "False"));
            HideWindow = Convert.ToBoolean(ReadConfig("HideWindow", "False"));
            AppendArguments = ReadConfig("AppendArguments", "");
            PrependArguments = ReadConfig("PrependArguments", "");
            FinalArguments = (PrependArguments + " " + OriginalArguments + " " + AppendArguments).Trim();
            string[] EnvVarToCreate = ReadConfig("EnvVarToCreate", "").Split(';');
            foreach (string EnvVar in EnvVarToCreate) 
            {
                if (EnvVar != "")
                {
                    // Read the related value
                    string envarconfig = ReadConfig("EnvVar_" + EnvVar, "");
                    EnvVar variable = new EnvVar(EnvVarAction.Create, EnvVar, envarconfig);
                    Variables.Add(variable);
                }
            }
            string[] EnvVarToAppend = ReadConfig("EnvVarToAppend", "").Split(';');
            foreach (string EnvVar in EnvVarToAppend)
            {
                // Read the related value
                if (EnvVar != "")
                {
                    string envarconfig = ReadConfig("EnvVar_" + EnvVar, "");
                    EnvVar variable = new EnvVar(EnvVarAction.Append, EnvVar, envarconfig);
                    Variables.Add(variable);
                }

            }
            string[] EnvVarToPrepend = ReadConfig("EnvVarToPrepend", "").Split(';');
            foreach (string EnvVar in EnvVarToPrepend)
            {
                if (EnvVar != "")
                {
                    // Read the related value
                    string envarconfig = ReadConfig("EnvVar_" + EnvVar, "");
                    EnvVar variable = new EnvVar(EnvVarAction.Prepend, EnvVar, envarconfig);
                    Variables.Add(variable);
                }
            }
        }
        private String ReadConfig(string keyname, string defaultvalue)
        {
            String value = defaultvalue;
            try
            {
                String keyvalue = ConfigurationManager.AppSettings[keyname];
                value = keyvalue;
            }
            catch
            {

            }
            if (value == "") {
                value = defaultvalue;
            }
            return value;
        }
        public void Execute()
        {
            // run the requested program

            switch (Target.ProgramType) 
            {
                case ProgramType.Executable:
                    ProcessToRun = Target.ToLaunchFullPath;
                    ProcessArguments = FinalArguments;
                    break;
                case ProgramType.Command:
                    ProcessToRun = Target.ToLaunchFullPath;
                    ProcessArguments = FinalArguments;
                    break;
                case ProgramType.VBScript:
                    ProcessToRun = "Wscript.exe";
                    ProcessArguments = $"//B //Nologo " + Target.ToLaunchFullPath;
                    if (FinalArguments != "")
                    {
                        ProcessArguments = ProcessArguments + " " + FinalArguments;
                    }
                    break;
                case ProgramType.PowershellScript:
                    ProcessToRun = "powershell.exe";
                    ProcessArguments = $"-File \"" + Target.ToLaunchFullPath + "\"";
                    if (FinalArguments != "")
                    {
                        ProcessArguments = ProcessArguments + " " + FinalArguments;
                    }
                    break;
                case ProgramType.Unknown:
                    return;
                default:
                    return;
            }
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = ProcessToRun,
                WorkingDirectory = Target.WorkingDir,
                Arguments = ProcessArguments,
                UseShellExecute = false,
                CreateNoWindow = HideWindow
            };

            if (Verbose == true) {
                // Show the summary window
                DialogResult result;
                SumaryWindow SumaryWindow = new SumaryWindow(this);
                Application.Run(SumaryWindow);
                result = SumaryWindow.Result;
                if (result != DialogResult.OK) 
                {
                    ShouldLaunch = false;
                }
            }
            if (ShouldLaunch == true)
            {
                // Start the process
                Process process = Process.Start(psi);
                if (Wait == true)
                {
                    process.WaitForExit();
                }
            }
            
        }
    }
    public class EnvVar
    { 
        public EnvVarAction Action = EnvVarAction.Create;
        public String Name;
        public String Value;
        public EnvVar(EnvVarAction action,string name,string value) 
        {
            Action = action;
            Name = name;
            Value = DecodeCurrDir(value);
            Set();
        }
        public EnvVar(string name, string value)
        {
            new EnvVar (EnvVarAction.Create, name,value);
        }
        private void Set()
        {
            // set the environment variable
            EnvironmentVariableTarget target = EnvironmentVariableTarget.Process;
            string currvalue = Environment.GetEnvironmentVariable(Name);
            switch (Action) 
            {
                case EnvVarAction.Create:
                    Environment.SetEnvironmentVariable(Name, Value, target);
                    break;
                case EnvVarAction.Append:
                    Environment.SetEnvironmentVariable(Name, currvalue + ";" + Value, target);
                    break;
                case EnvVarAction.Prepend:
                    Environment.SetEnvironmentVariable(Name, Value + ";" + currvalue, target);
                    break;
                default:
                    break;
            }
            currvalue = Environment.GetEnvironmentVariable(Name);
        }
        private string DecodeCurrDir(string value)
        {
            // if the value contains %CurrDir% then decode the directory
            if (value.ToLower().Contains("%currdir%"))
            {
                string[] values = value.Split(';');
                List<string> newvalues = new List<string>();
                Process process = Process.GetCurrentProcess();
                string currentDirectory = Path.GetDirectoryName(process.MainModule.FileName);
                foreach (String valString in values)
                {
                    string fullpathtest = valString.ToLower().Replace("%currdir%", currentDirectory);
                    newvalues.Add(Path.GetFullPath(fullpathtest));
                }
                return String.Join(";", newvalues);
            }
            else
            {
                return value;
            }
        }
    }
    public class ProgramInfo
    {
        public Boolean Exist = false;
        public String ToLaunch;
        public String ToLaunchFullPath;
        public String WorkingDir;
        public ProgramType ProgramType = ProgramType.Unknown;
        public ProgramInfo(string toLaunch,string workingDir)
        {
            // Handle the process
            ToLaunch = toLaunch;
            ToLaunchFullPath = GetFileFullPath(toLaunch);
            if (Exist != false) 
            {
                ProgramType = GetProgramType(ToLaunchFullPath);
            }
            // Handle the working dir
            if (workingDir == "")
            {
                WorkingDir = Path.GetDirectoryName(workingDir);
            }
            else
            {
                WorkingDir = GetFolderFullPath(workingDir);
            }
        }
        private String GetFileFullPath(string pathstring)
        {
            if (pathstring.ToLower().Contains("%currdir%"))
            {
                Process process = Process.GetCurrentProcess();
                string currentDirectory = Path.GetDirectoryName(process.MainModule.FileName);
                pathstring = pathstring.ToLower().Replace("%currdir%", currentDirectory);
            }
            pathstring = DecodeCurrDir(pathstring);
            // Translate the path
            String fullpath = "";
            try
            {
                fullpath = Path.GetFullPath(pathstring);
            }
            catch { }
            // check if the path exist
            Exist = System.IO.File.Exists(fullpath);
            if (Exist != true)
            {
                // somehow depending from where the exe is called the getfullpath can fail due to working directory
                // try to combine the current
                Process process = Process.GetCurrentProcess();
                string currentDirectory = Path.GetDirectoryName(process.MainModule.FileName);
                string fullpathtest = Path.Combine(currentDirectory, pathstring);
                try
                {
                    fullpath = Path.GetFullPath(fullpathtest);
                }
                catch { }
                Exist = System.IO.File.Exists(fullpath);
            }
            
            if (Exist != true)
            {
                fullpath = "";
                // the path does not exist search if it exist in one of the path in %PATH%
                string[] pathDirectories = Environment.GetEnvironmentVariable("PATH").Split(Path.PathSeparator);

                // Search for the application within the PATH directories
                foreach (string directory in pathDirectories)
                {
                    string fullpathtest = Path.Combine(directory, pathstring);
                    if (File.Exists(fullpathtest))
                    {
                        fullpath = fullpathtest;
                    }
                }
                Exist = System.IO.Directory.Exists(fullpath);
            }
            
            return fullpath;
        }
        private String GetFolderFullPath(string workingDirString)
        {
            if (workingDirString.ToLower().Contains("%currdir%"))
            {
                Process process = Process.GetCurrentProcess();
                string currentDirectory = Path.GetDirectoryName(process.MainModule.FileName);
                workingDirString = workingDirString.ToLower().Replace("%currdir%", currentDirectory);
            }
            String fulldir = "";
            Boolean workingDirExist = false;
            // Translate the path
            try
            {
                fulldir = Path.GetFullPath(workingDirString);
            }
            catch { }
            // check if the path exist
            workingDirExist = System.IO.Directory.Exists(fulldir);
            if (workingDirExist != true)
            {
                // somehow depending from where the exe is called the getfullpath can fail due to working directory
                // try to combine the current
                Process process = Process.GetCurrentProcess();
                string currentDirectory = Path.GetDirectoryName(process.MainModule.FileName);
                string fullpathtest = Path.Combine(currentDirectory, workingDirString);
                try
                {
                    fulldir = Path.GetFullPath(fullpathtest);
                }
                catch { }
                workingDirExist = System.IO.Directory.Exists(fulldir);
            }
            if (workingDirExist == true)
            {
                return fulldir;
            }
            else
            {
                return "";
            }
            

        }
        private ProgramType GetProgramType(string fullPath) 
        {
            string Extension = Path.GetExtension(fullPath);
            switch (Extension.ToLower())
            {
                case ".exe":
                    return ProgramType.Executable;
                case ".bat":
                    return ProgramType.Command;
                case ".cmd":
                    return ProgramType.Command;
                case ".vbs":
                    return ProgramType.VBScript;
                case ".ps1":
                    return ProgramType.PowershellScript;
                default:
                    return ProgramType.Unknown;
            }
            
        }
        private String DecodeCurrDir(string path)
        {
            if (path.ToLower().Contains("%currdir%"))
            {
                Process process = Process.GetCurrentProcess();
                string currentDirectory = Path.GetDirectoryName(process.MainModule.FileName);
                string fullpathtest = path.ToLower().Replace("%currdir%", currentDirectory);
                return Path.GetFullPath(fullpathtest);
            }
            else
            {
                return path;
            }
        }
    }
}
