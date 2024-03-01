using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace UniversalLauncher
{
    public partial class DetailWindow : Form
    {
        public ULObject ULObject;
        public DialogResult Result;
        public Panel Mainpanel = new Panel();
        public Label Title = new Label();
        public TextBox ProcessToStartValuetxtbx = new TextBox();
        public TextBox WorkingDirValuetxtbx = new TextBox();
        public TextBox PrependArgstxtbx = new TextBox();
        public TextBox AppendArgstxtbx = new TextBox();
        public TextBox CalcArgstxtbx = new TextBox();
        public CheckBox Hidechkbx = new CheckBox();
        public CheckBox Waitchkbx = new CheckBox();
        public ComboBox VarNamecombo = new ComboBox();
        public RadioButton Createradiobtn = new RadioButton() { Text = "Create" };
        public RadioButton Prependradiobtn = new RadioButton() { Text = "Preppend" };
        public RadioButton Appendradiobtn = new RadioButton() { Text = "Append" };
        public TextBox VarValuetxtbx = new TextBox();
        public DetailWindow(ULObject ulobject)
        {
            InitializeComponent();
            BuildForm(ulobject);
        }
        public void BuildForm(ULObject ulobject)
        {
            ULObject = ulobject;
            Controls.Add(Mainpanel);
            // Build the form
            // Form option
            this.Font = new Font("Segoe UI", 8);
            // to do
            Text = "Universal Launcher Verbose Window - " + ULObject.Launcher.Name.Replace(ULObject.Launcher.Extension,"");
            Width = 950;
            Height = 800;
            SizeGripStyle = SizeGripStyle.Hide;
            MaximizeBox = false;
            MinimizeBox = false;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            StartPosition = FormStartPosition.CenterScreen;
            // Create the Panel
            Mainpanel.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
            
            int leftAlignLabel = 20;
            int leftAlignValue = 280;
            int topMerging = 13;
            int objectgap = 35;
            Size TextBoxSize = new Size(600,15);
            Font labelfont = new Font("Segoe UI", 12, FontStyle.Bold);
            // Create the title
            Title.Text = ulobject.Launcher.Name;
            Title.Font = new Font("Segoe UI",14,FontStyle.Underline | FontStyle.Bold);
            Title.Location = new Point(leftAlignLabel, topMerging);
            Title.AutoSize = true;
            Mainpanel.Controls.Add(Title);
            // Create the current process information
            Label thisprocessPathNamelbl = new Label()
            {
                Text = "Path:",
                Location = new Point(leftAlignLabel, topMerging + (2 * objectgap)),
                AutoSize = true,
                Font = labelfont
            };
            Mainpanel.Controls.Add(thisprocessPathNamelbl);
            Label thisprocessPathValuelbl = new Label()
            {
                Text = ULObject.Launcher.Directory.FullName,
                Location = new Point(leftAlignValue, topMerging + (2 * objectgap)),
                AutoSize = true
            };
            Mainpanel.Controls.Add(thisprocessPathValuelbl);
            Label thisprocessArgumentsReceivedlbl = new Label()
            {
                Text = "Arguments:",
                Location = new Point(leftAlignLabel, thisprocessPathValuelbl.Location.Y + objectgap),
                AutoSize = true,
                Font = labelfont
            };
            Mainpanel.Controls.Add(thisprocessArgumentsReceivedlbl);
            Label thisprocessArgumentsReceivedValuelbl = new Label()
            {
                Text = ULObject.LauncherArguments,
                Location = new Point(leftAlignValue, thisprocessPathValuelbl.Location.Y + objectgap),
                AutoSize = true
            };
            Mainpanel.Controls.Add(thisprocessArgumentsReceivedValuelbl);
            Label ProcessToStartlbl = new Label()
            {
                Text = "To start:",
                Location = new Point(leftAlignLabel, thisprocessArgumentsReceivedValuelbl.Location.Y + objectgap),
                AutoSize = true,
                Font = labelfont
            };
            Mainpanel.Controls.Add(ProcessToStartlbl);

            ProcessToStartValuetxtbx.Location = new Point(leftAlignValue, thisprocessArgumentsReceivedValuelbl.Location.Y + objectgap);
            ProcessToStartValuetxtbx.Size = TextBoxSize;
            ProcessToStartValuetxtbx.Text = ULObject.ProcessToStart.FullName;
            Mainpanel.Controls.Add(ProcessToStartValuetxtbx);
            Label ProcessWorkingDirectorylbl = new Label()
            {
                Text = "Working Dir:",
                Location = new Point(leftAlignLabel, ProcessToStartlbl.Location.Y + objectgap),
                AutoSize = true,
                Font = labelfont
            };
            Mainpanel.Controls.Add(ProcessWorkingDirectorylbl);
            WorkingDirValuetxtbx.Location = new Point(leftAlignValue, ProcessToStartlbl.Location.Y + objectgap);
            WorkingDirValuetxtbx.Size = TextBoxSize;
            WorkingDirValuetxtbx.Text = ULObject.WorkingDirectory;
            Mainpanel.Controls.Add(WorkingDirValuetxtbx);
            Label PrependArgumentslbl = new Label()
            {
                Text = "Prepend Argument(s):",
                Location = new Point(leftAlignLabel, ProcessWorkingDirectorylbl.Location.Y + objectgap),
                AutoSize = true,
                Font = labelfont
            };
            Mainpanel.Controls.Add(PrependArgumentslbl);
            PrependArgstxtbx.Location = new Point(leftAlignValue, ProcessWorkingDirectorylbl.Location.Y + objectgap);
            PrependArgstxtbx.Size = TextBoxSize;
            PrependArgstxtbx.Text = ULObject.ArgumentsToPrepend;
            PrependArgstxtbx.TextChanged += UpdateArguments;
            Mainpanel.Controls.Add(PrependArgstxtbx);
            Label AppendArgumentslbl = new Label()
            {
                Text = "Prepend Argument(s):",
                Location = new Point(leftAlignLabel, PrependArgumentslbl.Location.Y + objectgap),
                AutoSize = true,
                Font = labelfont
            };
            Mainpanel.Controls.Add(AppendArgumentslbl);
            AppendArgstxtbx.Location = new Point(leftAlignValue, PrependArgumentslbl.Location.Y + objectgap);
            AppendArgstxtbx.Size = TextBoxSize;
            AppendArgstxtbx.Text = ULObject.ArgumentsToAppend;
            AppendArgstxtbx.TextChanged += UpdateArguments;
            Mainpanel.Controls.Add(AppendArgstxtbx);
            Label CalculatedArgumentslbl = new Label()
            {
                Text = "Calculated Argument(s):",
                Location = new Point(leftAlignLabel, AppendArgumentslbl.Location.Y + objectgap),
                AutoSize = true,
                Font = labelfont
            };
            Mainpanel.Controls.Add(CalculatedArgumentslbl);
            CalcArgstxtbx.Location = new Point(leftAlignValue, AppendArgumentslbl.Location.Y + objectgap);
            CalcArgstxtbx.Size = TextBoxSize;
            CalcArgstxtbx.ReadOnly = true;
            Mainpanel.Controls.Add(CalcArgstxtbx);
            Label Hidelbl = new Label()
            {
                Text = "Hide Window:",
                Location = new Point(leftAlignLabel, CalculatedArgumentslbl.Location.Y + objectgap),
                AutoSize = true,
                Font = labelfont
            };
            Mainpanel.Controls.Add(Hidelbl);
            Hidechkbx.Location = new Point(leftAlignValue, CalculatedArgumentslbl.Location.Y + objectgap);
            Hidechkbx.Checked = ULObject.Hide;
            Mainpanel.Controls.Add(Hidechkbx);
            Label Waitlbl = new Label()
            {
                Text = "Wait process end:",
                Location = new Point(leftAlignLabel, Hidelbl.Location.Y + objectgap),
                AutoSize = true,
                Font = labelfont
            };
            Mainpanel.Controls.Add(Waitlbl);
            Waitchkbx.Location = new Point(leftAlignValue, Hidelbl.Location.Y + objectgap);
            Waitchkbx.Checked = ULObject.Wait;
            Mainpanel.Controls.Add(Waitchkbx);
            Label EnvironmentNamelbl = new Label()
            {
                Text = "Environment Variable Name:",
                Location = new Point(leftAlignLabel, Waitlbl.Location.Y + objectgap),
                AutoSize = true,
                Font = labelfont
            };
            Mainpanel.Controls.Add(EnvironmentNamelbl);
            foreach (EnvironmentVariable var in ULObject.EnvironmentVariables)
            {
                VarNamecombo.Items.Add(var.Name);
            }
            VarNamecombo.SelectedIndex = 0;
            VarNamecombo.SelectedIndexChanged += varName_On_Change;
            VarNamecombo.Width = 200;
            VarNamecombo.Location = new Point(leftAlignValue, Waitlbl.Location.Y + objectgap);
            Mainpanel.Controls.Add(VarNamecombo);
            Label EnvironmentActionlbl = new Label()
            {
                Text = "Environment Variable Action:",
                Location = new Point(leftAlignLabel, EnvironmentNamelbl.Location.Y + objectgap),
                AutoSize = true,
                Font = labelfont
            };
            Mainpanel.Controls.Add(EnvironmentActionlbl);
            Createradiobtn.Location = new Point(leftAlignValue, EnvironmentNamelbl.Location.Y + objectgap);
            Createradiobtn.AutoSize = true;
            Prependradiobtn.Location = new Point(Createradiobtn.Location.X + Createradiobtn.Width , EnvironmentNamelbl.Location.Y + objectgap);
            Prependradiobtn.AutoSize = true;
            Appendradiobtn.Location = new Point(Prependradiobtn.Location.X + Prependradiobtn.Width, EnvironmentNamelbl.Location.Y + objectgap);
            Appendradiobtn.AutoSize = true;
            Mainpanel.Controls.Add(Createradiobtn);
            Mainpanel.Controls.Add(Prependradiobtn);
            Mainpanel.Controls.Add(Appendradiobtn);
            Label EnvironmentValuelbl = new Label()
            {
                Text = "Environment Variable Value:",
                Location = new Point(leftAlignLabel, EnvironmentActionlbl.Location.Y + objectgap),
                AutoSize = true,
                Font = labelfont
            };
            Mainpanel.Controls.Add(EnvironmentValuelbl);
            VarValuetxtbx.Location = new Point(leftAlignValue, EnvironmentActionlbl.Location.Y + objectgap);
            VarValuetxtbx.Size = TextBoxSize;
            Mainpanel.Controls.Add(VarValuetxtbx);
            Button startCMD = new Button() { Text = "Start CMD", Size = new Size(200, 32) };
            startCMD.Location = new Point(leftAlignLabel, EnvironmentValuelbl.Location.Y + objectgap);
            startCMD.Click += startCMD_On_Click;
            Mainpanel.Controls.Add(startCMD);
            Button startPS = new Button() { Text = "Start PS", Size = new Size(200, 32) };
            startPS.Location = new Point(startCMD.Width + startCMD.Location.X, EnvironmentValuelbl.Location.Y + objectgap);
            startPS.Click += startPS_On_Click;
            Mainpanel.Controls.Add(startPS);

            // windows buttons
            Button CancelBTN = new Button() { Text = "Cancel", Size = new Size(200, 32) };
            CancelBTN.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            // CancelBTN.Location = new Point(leftAlignLabel, EnvironmentValuelbl.Location.Y + objectgap);
            CancelBTN.DialogResult = DialogResult.Cancel;
            CancelBTN.Click += CancelBTN_Click;
            Button OKBtn = new Button() { Text = "OK Run", Size = new Size(200, 32) };
            OKBtn.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            // OKBtn.Location = new Point(leftAlignLabel, EnvironmentValuelbl.Location.Y + objectgap);
            OKBtn.DialogResult = DialogResult.Cancel;
            OKBtn.Click += OKBtn_Click;
            FlowLayoutPanel panel = new FlowLayoutPanel();
            panel.Dock = DockStyle.Bottom;
            panel.AutoSize = true;
            panel.FlowDirection = FlowDirection.RightToLeft;
            panel.Controls.Add(OKBtn);
            panel.Controls.Add(CancelBTN);
            Mainpanel.Controls.Add(panel);
            // force resize of the window
            this.Resize += DetailWindow_On_Resize;
            DetailWindow_On_Resize(this, EventArgs.Empty);
            UpdateArguments(this, EventArgs.Empty);
            varName_On_Change(VarNamecombo, EventArgs.Empty);
            FormClosed += DetailWindow_On_Close;

        }
        private void startCMD_On_Click(object sender, EventArgs e) {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                UseShellExecute = false,
                CreateNoWindow = false
            };
            Process process = Process.Start(psi);
        }
        private void startPS_On_Click(object sender, EventArgs e) {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                UseShellExecute = false,
                CreateNoWindow = false
            };
            Process process = Process.Start(psi);
        }
        private void CancelBTN_Click(object sender, EventArgs e)
        {
            // Close the form when Cancel button is clicked
            this.Close();
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            // Close the form when OK button is clicked
            this.Close();
        }
        private void varName_On_Change(object sender,EventArgs e) {
            System.Windows.Forms.ComboBox combo = (System.Windows.Forms.ComboBox)sender;
            try
            {
                string SelectItem = (string)combo.SelectedItem;
                VarValuetxtbx.Text = ULObject.EnvironmentVariables.First(item => item.Name == SelectItem).Value;
                switch(ULObject.EnvironmentVariables.First(item => item.Name == SelectItem).Action)
                {
                    case EnvVarAction.Create:
                        Createradiobtn.Checked = true;
                        Prependradiobtn.Checked = false;
                        Appendradiobtn.Checked = false;
                        break;
                    case EnvVarAction.Prepend:
                        Createradiobtn.Checked = false;
                        Prependradiobtn.Checked = true;
                        Appendradiobtn.Checked = false;
                        break;
                    case EnvVarAction.Append:
                        Createradiobtn.Checked = false;
                        Prependradiobtn.Checked = false;
                        Appendradiobtn.Checked = true;
                        break;
                }
            }
            catch { }
        }
        private void DetailWindow_On_Resize(object sender, EventArgs e)
        {
            // Adjust the size and location of the panel to match the form
            Mainpanel.Size = new System.Drawing.Size(this.ClientSize.Width - 20, this.ClientSize.Height - 20);
            Mainpanel.Location = new System.Drawing.Point(10, 10);
        }
        private void UpdateArguments(object sender,EventArgs e)
        {
            string[] array = { PrependArgstxtbx.Text.Trim(), ULObject.LauncherArguments.Trim(), AppendArgstxtbx.Text.Trim() };
            CalcArgstxtbx.Text = String.Join(" ", array).Trim();
        }
        private void DetailWindow_On_Close(object sender, FormClosedEventArgs e)
        {
            if (Result == DialogResult.OK)
            {
                Boolean _isIncorrectValue = false;
                List<string> _incorrectValues = new List<string>();
                FileInfo _processToStart = new FileInfo(ProcessToStartValuetxtbx.Text);
                if (_processToStart.Exists)
                {
                    ULObject.ProcessToStart = _processToStart;
                }
                else
                {
                    _isIncorrectValue = true;
                    _incorrectValues.Add(ProcessToStartValuetxtbx.Text + ": Does not exist !");
                }
                if (_isIncorrectValue)
                {
                    Helper.ErrorMessage(@"Some value are incorrect and will be ignored:

" + String.Join(@"
", _incorrectValues), "Incorrect Values");
                }
            }
            else if (e.CloseReason!= CloseReason.UserClosing)
            {
                // form has been closed using the cancel button (exclude the cross from this message)
                Helper.ErrorMessage(@"Launch stopped by user !

Exitting...", "Launch canceled");
            }
            else
            {
                // the form has been closed using the cross => do not save the value but still launch
                Result = DialogResult.OK;
            }
        }
    }
}
