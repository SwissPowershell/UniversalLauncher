using System;
using System.Collections;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace UniversalLauncher
{
    public partial class SumaryWindow : Form
    {
        public DialogResult Result;
        ProgramConfig Config;
        public SumaryWindow(ProgramConfig config)
        {
            InitializeComponent();
            SetValues(config);
        }
        public SumaryWindow() {
            InitializeComponent();
        }
        private void SetValues(ProgramConfig config)
        {
            Config = config;
            Process process = Process.GetCurrentProcess();
            Titlelbl.Text = Path.GetFileName(process.MainModule.FileName);
            CurrentProcesstxtbx.Text = process.MainModule.FileName;
            PathToStarttxtbx.Text = config.Target.ToLaunchFullPath;
            Argumentstxtbx.Text = config.FinalArguments;
            FinalCommandLinetxtbx.Text = config.ProcessToRun + " " + config.ProcessArguments;
            WorkingDirtxtbx.Text = config.Target.WorkingDir;
            Waitchkbx.Checked = config.Wait;
            HideWindowchkbx.Checked = config.HideWindow;
            HideFileNotFoundchkbx.Checked = config.HideNotExistError;
            foreach (EnvVar Var in config.Variables) {
                Variablescombo.Items.Add(Var.Name);
            }
            Variablescombo.DropDownStyle = ComboBoxStyle.DropDownList;
            Variablescombo.SelectedIndex = 0;
        }

        private void Okbtn_Click(object sender, EventArgs e)
        {
            Result = DialogResult.OK;
            this.Close();
        }

        private void Cancelbtn_Click(object sender, EventArgs e)
        {
            Result = DialogResult.Cancel;
            this.Close();
        }

        private void Variablescombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.ComboBox combo = (System.Windows.Forms.ComboBox)sender;
            try
            {
                string SelectItem = (string)combo.SelectedItem;
                VariableValuetxtbx.Text = Config.Variables.First(item => item.Name == SelectItem).Value;
                VariableTypetxtbx.Text = Config.Variables.Find(item => item.Name == SelectItem).Action.ToString();
            }
            catch { }
            

        }

        private void SumaryWindow_Load(object sender, EventArgs e)
        {

        }
    }

}
