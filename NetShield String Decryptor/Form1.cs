using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetShield_String_Decryptor
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private void TextBox1DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }
        private void TextBox2DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }
        private void TextBox2DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                Array array = (Array)e.Data.GetData(DataFormats.FileDrop);
                if (array != null)
                {
                    string text = array.GetValue(0).ToString();
                    int num = text.LastIndexOf(".", StringComparison.Ordinal);
                    if (num != -1)
                    {
                        string text2 = text.Substring(num);
                        text2 = text2.ToLower();
                        if (text2 == ".txt")
                        {
                            Activate();
                            textBox2.Text = text;
                            int num2 = text.LastIndexOf("\\", StringComparison.Ordinal);
                            if (num2 != -1)
                            {
                                DirectoryName = text.Remove(num2, text.Length - num2);
                            }
                            if (DirectoryName.Length == 2)
                            {
                                DirectoryName += "\\";
                            }
                        }
                    }
                }
            }
            catch
            {
            }
        }
        private void TextBox1DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                Array array = (Array)e.Data.GetData(DataFormats.FileDrop);
                if (array != null)
                {
                    string text = array.GetValue(0).ToString();
                    int num = text.LastIndexOf(".", StringComparison.Ordinal);
                    if (num != -1)
                    {
                        string text2 = text.Substring(num);
                        text2 = text2.ToLower();
                        if (text2 == ".exe" || text2 == ".dll")
                        {
                            Activate();
                            textBox1.Text = text;
                            int num2 = text.LastIndexOf("\\", StringComparison.Ordinal);
                            if (num2 != -1)
                            {
                                DirectoryName = text.Remove(num2, text.Length - num2);
                            }
                            if (DirectoryName.Length == 2)
                            {
                                DirectoryName += "\\";
                            }
                        }
                    }
                }
            }
            catch
            {
            }
        }
        string DirectoryName = "";
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Browse for target assembly";
            openFileDialog.InitialDirectory = "c:\\";
            if (DirectoryName != "")
            {
                openFileDialog.InitialDirectory = this.DirectoryName;
            }
            openFileDialog.Filter = "All files (*.exe,*.dll)|*.exe;*.dll";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = openFileDialog.FileName;
                textBox1.Text = fileName;
                int num = fileName.LastIndexOf("\\", StringComparison.Ordinal);
                if (num != -1)
                {
                    DirectoryName = fileName.Remove(num, fileName.Length - num);
                }
                if (DirectoryName.Length == 2)
                {
                    DirectoryName += "\\";
                }
            }
        }
        public void StringDecryptor(ModuleDefMD module)
        {
            foreach (TypeDef type in module.Types)
            {
               foreach(MethodDef method in type.Methods)
                {
                    if(method.HasBody)
                    {
                        if(method.Body.HasInstructions)
                        {
                            for (int i = 0; i < method.Body.Instructions.Count; i++)
                            {
                                if (method.Body.Instructions[i].IsLdcI4() && method.Body.Instructions[i + 1].OpCode == OpCodes.Call && method.Body.Instructions[i + 1].Operand.ToString().Contains("System.String"))
                                {
                                    int index = (int)method.Body.Instructions[i].GetLdcI4Value();
                                    string[] data = File.ReadAllLines(textBox2.Text);
                                    string result = data[index];
                                    string corrected = result.Replace("\"", "");
                                    method.Body.Instructions[i].OpCode = OpCodes.Ldstr;
                                    method.Body.Instructions[i].Operand = corrected;
                                   method.Body.Instructions[i + 1].OpCode = OpCodes.Nop;
                                }
                            }
                        }
                    }
                }
                    
            }

        }
        private void button2_Click(object sender, EventArgs e)
        {
            ModuleDefMD module = ModuleDefMD.Load(textBox1.Text);
            StringDecryptor(module);
            string text2 = Path.GetDirectoryName(textBox1.Text);
            if (!text2.EndsWith("\\"))
            {
                text2 += "\\";
            }
            string path = text2 + Path.GetFileNameWithoutExtension(textBox1.Text) + "_Decrypter" +
                          Path.GetExtension(textBox1.Text);
            var opts = new ModuleWriterOptions(module);
            opts.Logger = DummyLogger.NoThrowInstance;
            module.Write(path, opts);
        }
    }
}
