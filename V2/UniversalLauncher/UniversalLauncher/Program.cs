using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using System.Threading;

namespace UniversalLauncher
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void tMain(string[] args)
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
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ULObject universalLauncher = new ULObject(args);
            universalLauncher.ShowVerboseWindow();
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
    public class ULObject
    {
        public Guid ObjectID = new Guid();
       
        public FileInfo Launcher = new FileInfo((Process.GetCurrentProcess()).MainModule.FileName);
        public String LauncherArguments {
            get { return string.Join(" ", this.LauncherArgumentsArray); }
        }
        private string[] LauncherArgumentsArray;
        public FileInfo ProcessToStart;
        public String WorkingDirectory;
        public String ArgumentsToPrepend = Helper.ReadConfig("PrependArguments");
        public String ArgumentsToAppend = Helper.ReadConfig("AppendArguments");
        public List<EnvironmentVariable> EnvironmentVariables;
        private Boolean isDebug = Convert.ToBoolean(Helper.ReadConfig("Debug", "False"));
        public Boolean Verbose = Convert.ToBoolean(Helper.ReadConfig("Verbose", "False"));
        public Boolean Hide = Convert.ToBoolean(Helper.ReadConfig("Hide", "False"));
        public Boolean Wait = Convert.ToBoolean(Helper.ReadConfig("Wait", "False"));
        public Boolean Reporting = Convert.ToBoolean(Helper.ReadConfig("Reporting", "False"));
        public String ReportingPath = Helper.ReadConfig("ReportingPath", "");
        public ULObject(string[] args)
        {
            LauncherArgumentsArray = args;
            RetrieveArgumentModifier(); // you can override the debug and verbose config option
            // Decode the "Start" config object
            String toStart = Helper.ReadConfig("Start");
            String toStartDecoded = DecodePath(toStart,true);
            if (toStartDecoded == "")
            {
                Helper.ErrorMessage(@"Target not found

" + toStart + @"

Can't be found !", "Path not found");
                return;
            }
            else
            {
                ProcessToStart = new FileInfo(toStartDecoded);
            }
            // Decode the "WorkingDirectory" config object
            WorkingDirectory = DecodePath(Helper.ReadConfig("WorkingDirectory"),false);
            // get the env var
            EnvironmentVariables = GetEnvironmentVariables();
            if (Reporting || ReportingPath == "") { 
                if (isDebug)
                {
                    Helper.DebugMessage(@"
Reporting is [True] but not ReportingPath config set...
Reporting will be set to [False].", "Reporting config mismatch!");
                }
                Reporting = false; 
            }
            
        }
        public Boolean ShowVerboseWindow() 
        {
            if (Verbose)
            {
                DetailWindow SumaryWindow = new DetailWindow(this);
                Application.Run(SumaryWindow);
                string FullName = SumaryWindow.ULObject.ProcessToStart.FullName;
                return SumaryWindow.Result == DialogResult.OK;
            }
            else
            {
                return true;
            }
        }
        public void RetrieveArgumentModifier()
        {
            // Search for argument modifier in the format -+Debug+ or -+Verbose+ that can override the config file
            // allowed Debug,Verbose
            string resultDebug = LauncherArgumentsArray.FirstOrDefault(s => string.Equals(s, "-+debug+", StringComparison.OrdinalIgnoreCase));
            string resultVerbose = LauncherArgumentsArray.FirstOrDefault(s => string.Equals(s, "-+verbose+", StringComparison.OrdinalIgnoreCase));
            if (resultDebug != null)
            {
                // If found, remove it from the array
                LauncherArgumentsArray = LauncherArgumentsArray.Where(s => !string.Equals(s, "-+debug+", StringComparison.OrdinalIgnoreCase)).ToArray();
                isDebug = true;
            }
            if (resultVerbose != null)
            {
                // If found, remove it from the array
                LauncherArgumentsArray = LauncherArgumentsArray.Where(s => !string.Equals(s, "-+verbose+", StringComparison.OrdinalIgnoreCase)).ToArray();
                Verbose = true;
            }
        }
        public void Start() 
        {
            // Set the environment Variables
            // Report Process start if reporting
            // Start the process
            // Report Process stop if reporting
        }
        public String DecodePath(String path,Boolean isFile) 
        {
            Boolean pathExist = false;
            String inputstr = path;
            if (isFile){pathExist = System.IO.File.Exists(path); } else { pathExist = System.IO.Directory.Exists(path); }
            if (pathExist)
            {
                path = Path.GetFullPath(path);
                if (isDebug)
                {
                    Helper.DebugMessage(@"The input string: 

" + inputstr + @"

As been found in:

" + path, "DecodePath - Path is full path!");
                }
                return path;
            }
            // Search for the given path
            // replace currdir by the current process directory
            path = Helper.DecodeCurrDir(path);
            // replace .\ by the current process directory
            path = Regex.Replace(path, @"^\.\\(.*)$", Launcher.DirectoryName + @"\$1" );
            path = Path.GetFullPath(path);
            if (isFile) { pathExist = System.IO.File.Exists(path); } else { pathExist = System.IO.Directory.Exists(path); }
            if (pathExist)
            {
                if (isDebug) { 
                    Helper.DebugMessage(@"The input string:

" + inputstr + @"

As been found in:

" + path, "DecodePath - Path is relative path!"); 
                }   
                return path;
            }
            else 
            {
                // the path exist search if it exist in one of the %path% directory
                string[] pathDirectories = Environment.GetEnvironmentVariable("PATH").Split(Path.PathSeparator);
                // Search for the application within the PATH directories
                foreach (string directory in pathDirectories)
                {
                    string fullpathtest = Path.Combine(directory, inputstr);
                    fullpathtest = Path.GetFullPath(fullpathtest);
                    if (isFile) { pathExist = System.IO.File.Exists(path); } else { pathExist = System.IO.Directory.Exists(path); }
                    if (pathExist)
                    {
                        if (isDebug)
                        {
                            Helper.DebugMessage(@"The input string: 
" + inputstr + @"
As been found in:
" + fullpathtest, "DecodePath - Path exist in a default directory");
                        }
                        return fullpathtest;
                    }
                }
                if (isDebug)
                {
                    Helper.DebugMessage(@"The input string: 
" + inputstr + @"
Can't be found anywhere, Tested:
" + Launcher.DirectoryName + "" + @"
" + String.Join(@"
",pathDirectories), "DecodePath - Path does not exist");
                }
                return "";
            }
            
        }
        public List<EnvironmentVariable> GetEnvironmentVariables()
        {
            List<EnvironmentVariable> retval = new List<EnvironmentVariable>();
            foreach (String envVarstr in Helper.ReadConfig("CreateEnvVar", "").Split(';'))
            {
                if (!(envVarstr == "" || envVarstr == null)){retval.Add(new EnvironmentVariable(EnvVarAction.Create, envVarstr));}
            }
            foreach (String envVarstr in Helper.ReadConfig("PrependEnvVar", "").Split(';'))
            {
                if (!(envVarstr == "" || envVarstr == null)){retval.Add(new EnvironmentVariable(EnvVarAction.Prepend, envVarstr));}
            }
            foreach (String envVarstr in Helper.ReadConfig("AppendEnvVar", "").Split(';'))
            {
                if (! (envVarstr == "" || envVarstr == null)){retval.Add(new EnvironmentVariable(EnvVarAction.Append, envVarstr));} 
            }
            return retval;
    }
        public void SetEnvironmentVariables(){ EnvironmentVariables.ForEach(envvar => envvar.Write(isDebug)); }
        public void ReportStart() { }
        public void ReportStop() { }
    }
    public class EnvironmentVariable
    {
        public EnvVarAction Action;
        public String Name;
        public String Value;
        public EnvironmentVariable(EnvVarAction action, String name)
        {
            Action = action;
            Name = name;
            string value = Helper.ReadConfig("EnvVar_" + name);
            Value = Helper.DecodeCurrDir(value);
        }
        public void Write(Boolean isDebug = false)
        {
            
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
            string newvalue = Environment.GetEnvironmentVariable(Name);
            if (isDebug)
            {
                Helper.DebugMessage(@"Environment Variable
Name: " + Name + @"
" + Action + ": " + Value + @"

New Value :
" + newvalue.Replace(";",@";
"), "Environment Variable " + Action);
            }
        }
    }
    public class Helper
    {
        public static String ReadConfig(String name,String defaultValue = "")
        {
            String keyvalue = "";
            try
            {
                keyvalue = ConfigurationManager.AppSettings[name];
            }
            catch
            {
                // the value did not exist
            }
            if (keyvalue == "" || keyvalue == null)
            {
                return defaultValue;
            }
            else
            {
                return keyvalue;
            }
            
        }
        public static void DebugMessage(String message,String title) 
        {
            MessageBox.Show(message, "Debug - " + title, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
        public static void ErrorMessage(String message, String title)
        {
            MessageBox.Show(message, "Error - " + title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        public static void Execute(String FileName,String WorkingDirectory,String Arguments,Boolean Hidden, Boolean Wait)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = FileName,
                WorkingDirectory = WorkingDirectory,
                Arguments = Arguments,
                UseShellExecute = false,
                CreateNoWindow = Hidden,
            };
            Process process = Process.Start(processStartInfo);
            if (Wait == true)
            {
                process.WaitForExit();
            }
            process.Dispose();
        }
        public static String DecodeCurrDir(String path) 
        {
            if (path.ToLower().Contains("%currdir%"))
            {
                FileInfo currprocess = new FileInfo((Process.GetCurrentProcess()).MainModule.FileName);
                path = Regex.Replace(path, @"(?i)^.*?%currdir%.*?$", currprocess.DirectoryName, RegexOptions.IgnoreCase);
                return path;
            }
            else
            {
                return path;
            }
            
        }
    }
    public class ReportingManager
    {
        private const string XmlFilePath = "report.xml";

        public static void ReportProcessStart(string processPath, string workingDirectory, string arguments, string userName)
        {
            XmlDocument doc = new XmlDocument();

            // Lock the XML file for exclusive access
            using (FileStream fileStream = new FileStream(XmlFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
            {
                fileStream.Lock(0, fileStream.Length);

                // Load existing XML file
                doc.Load(fileStream);

                // Create a new process element with a random GUID as the identifier
                XmlElement processElement = doc.CreateElement("Process");
                processElement.SetAttribute("ID", Guid.NewGuid().ToString());

                // Add child elements for the process details
                AddXmlElement(doc, processElement, "ProcessPath", processPath);
                AddXmlElement(doc, processElement, "WorkingDirectory", workingDirectory);
                AddXmlElement(doc, processElement, "Arguments", arguments);
                AddXmlElement(doc, processElement, "UserName", userName);
                AddXmlElement(doc, processElement, "StartTime", DateTime.Now.ToString());

                // Append the process element to the root
                doc.DocumentElement?.AppendChild(processElement);

                // Save the XML file
                doc.Save(fileStream);

                // Release the lock
                fileStream.Unlock(0, fileStream.Length);
            }
        }

        public static void ReportProcessStop(string processId, int errorLevel)
        {
            XmlDocument doc = new XmlDocument();

            // Lock the XML file for exclusive access
            using (FileStream fileStream = new FileStream(XmlFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
            {
                fileStream.Lock(0, fileStream.Length);

                // Load existing XML file
                doc.Load(fileStream);

                // Find the process element with matching ID
                XmlNode processNode = doc.SelectSingleNode($"//Process[@ID='{processId}']");
                if (processNode != null)
                {
                    // Update the StopTime, Elapsed, and ErrorLevel
                    processNode["StopTime"].InnerText = DateTime.Now.ToString();
                    DateTime startTime = DateTime.Parse(processNode["StartTime"].InnerText);
                    TimeSpan elapsedTime = DateTime.Now - startTime;
                    processNode["Elapsed"].InnerText = elapsedTime.ToString();
                    processNode["ErrorLevel"].InnerText = errorLevel.ToString();
                }

                // Save the XML file
                doc.Save(fileStream);

                // Release the lock
                fileStream.Unlock(0, fileStream.Length);
            }
        }

        private static void AddXmlElement(XmlDocument doc, XmlElement parent, string elementName, string value)
        {
            XmlElement element = doc.CreateElement(elementName);
            element.InnerText = value;
            parent.AppendChild(element);
        }
    }

}
