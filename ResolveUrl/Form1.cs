using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace ResolveUrl
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.textBox2.Text = @"C:\Users\Administrator\Desktop";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var originFileUrl = this.textBox1.Text;
            if (string.IsNullOrWhiteSpace(originFileUrl)
                || !File.Exists(originFileUrl))
            {
                MessageBox.Show("文件路径不正确!");
                return;
            }

            if (string.IsNullOrWhiteSpace(this.textBox2.Text)
                || !Directory.Exists(this.textBox2.Text))
            {
                MessageBox.Show("输出目录文件夹不正确!");
                return;
            }

            var urls = Excute(originFileUrl);
            if (urls.Count == 0)
            {
                MessageBox.Show("提取失败");
                return;
            }
            SaveFile(this.textBox2.Text, urls);
            MessageBox.Show($"执行结束，一共提取{urls.Count}条数据");
        }


        /// <summary>
        /// https://www.cnblogs.com/mooncher/p/3461603.html urlset 带了自定义的命名空间，需要加命名空间
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private List<string> Excute(string path)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            XmlNamespaceManager m = new XmlNamespaceManager(doc.NameTable);
            m.AddNamespace("bc", "http://www.sitemaps.org/schemas/sitemap/0.9");

            XmlElement? root = doc.DocumentElement;
            if (root == null) return new();

            XmlNodeList? nodeList = root.SelectNodes("/bc:urlset/bc:url/bc:loc",m);
            if (nodeList == null) return new();
            var urls = new List<string>();
            foreach (XmlNode node in nodeList)
            {
                var url = node.InnerText;
                if (!string.IsNullOrWhiteSpace(url))
                {
                    urls.Add(url);
                }
            }
            return urls;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "请选择文件路径";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Debug.WriteLine(dialog.SelectedPath);
                //savePath = dialog.SelectedPath;
                //textBox2.Text = savePath;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.Title = "选择文件夹";
            dialog.Filter = "xml 文件|*.xml";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                this.textBox1.Text = dialog.FileName;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var folderDialog = new FolderBrowserDialog();
            folderDialog.Description = "选择文件目录";

            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                this.textBox2.Text = folderDialog.SelectedPath;
            }
        }


        private void SaveFile(string outDir,List<string> urls)
        {
            var fileName = $"url_{DateTimeOffset.Now.ToUnixTimeSeconds()}.txt";
            var path = Path.Combine(outDir, fileName);
            File.WriteAllLines(path, urls);
        }
    }
}
