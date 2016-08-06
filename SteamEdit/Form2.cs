using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace SteamEdit
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private List<RegistryRow> listRegistry;

        private void ReadRegistryJson()
        {
            listRegistry = new List<RegistryRow>();

            using (StreamReader sr = new StreamReader("RegistryKeys.json"))
            {
                JsonTextReader reader = new JsonTextReader(sr);
                reader.SupportMultipleContent = true;

                while (true)
                {
                    if (!reader.Read())
                    {
                        break;
                    }

                    JsonSerializer serializer = new JsonSerializer();
                    RegistryRow row = serializer.Deserialize<RegistryRow>(reader);

                    listRegistry.Add(row);
                }
            }
        }

        // http://stackoverflow.com/questions/3923082/how-to-add-data-to-datagridview
        private void tabControl1_Enter(object sender, EventArgs e)
        {
            // data
            ReadRegistryJson();

            // use binding source to hold = data
            BindingSource binding = new BindingSource();
            binding.DataSource = listRegistry;

            // bind datagridview to binding source
            dataGridView1.DataSource = binding;

            // InitializeComponent();
            GetSteamPath();
            ReadACF(manifestPath);


        }


        public String acfText;
        public String steamPath;
        public String manifestPath;
        public String steamappsPath;


        /*
         * [HKEY_CURRENT_USER\SOFTWARE\Valve\Steam]
"Language"="english"
"SteamInstaller"="SteamSetup.exe"
"SteamExe"="c:/program files (x86)/steam/steam.exe"
"SteamPath"="c:/program files (x86)/steam"
"SuppressAutoRun"=dword:00000001
*/
        private void ShowErrorMessage(Exception e, string Title)
        {
            MessageBox.Show(e.Message,
                            Title
                            , MessageBoxButtons.OK
                            , MessageBoxIcon.Error);
        }


        private void Fail(string Message)
        {
            MessageBox.Show(Message, "Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }
        public void GetSteamPath()
        {
            textBoxSteamPath.Text = steamPath = Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Valve\\Steam", "SteamPath","").ToString().Replace('/', '\\');
            if (!Directory.Exists(steamPath))
            {
                Fail("Directory " + steamPath + " does not exist");
            }

            steamPath += "\\steamapps";
            if (!Directory.Exists(steamPath))
            {
                Fail("Directory " + steamPath + " does not exist");
            }

            manifestPath = steamPath + "\\appmanifest_271590.acf";
            if (!File.Exists(manifestPath))
            {
                Fail("File " + manifestPath + " does not exist");
            }
            textBoxManifestPath.Text = manifestPath;

            steamappsPath = steamPath + "\\common";
            if (!Directory.Exists(steamPath))
            {
                Fail("Directory " + steamappsPath + " does not exist");
            }

        }

        public String JsonFromACF(String input)
        {
            String[,] regexes = {
                {  @"^(\s*)(""[^\t]+"")$",                     "$1$2:",     "m"  },
                {  @"^(\s*)(""[^\t]+"")([\s]+)(""[^\t]+"")$",  "$1$2:$4,",  "m"  },
                {  @"}$",                                      "},",        "m"  },
                {  @",(\n\s*})",                               "$1",        ""   },
                {  @"^",                                       "{",         ""   },
                {  @",$",                                      "}",         ""   }
            };
            for (int x = 0; x < regexes.GetLength(0); x += 1)
            {
                string pattern = regexes[x, 0];
                string replacement = regexes[x, 1];
                Regex rgx = new Regex(pattern, (regexes[x, 2] == "m") ? RegexOptions.Multiline : RegexOptions.None);
                input = rgx.Replace(input, replacement);
            }

            return input;
        }

        public void ReadACF(String filename)
        {
            acfText = File.ReadAllText(filename);
            String input = acfText;
            String pattern = @"\t""installdir""\t\t""([^""]+)""";
            Match m = Regex.Match(input, pattern, RegexOptions.IgnoreCase);
            if (m.Success)
            {
                textBoxGta5Path.Text = m.Groups[1].Value;
            }
            JsonFromACF(acfText);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowHelp = true;
            openFileDialog1.FileName = "GTA5.exe";
            openFileDialog1.Filter = "GTA5 Executable|GTA5.exe"; // Text files (*.txt)|*.txt|All files (*.*)|*.*
            openFileDialog1.InitialDirectory = steamappsPath;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.textBoxGta5Path.Text = openFileDialog1.FileName;
            }
        }
    }
}
    


