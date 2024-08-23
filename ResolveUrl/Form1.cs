using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

            var tagName = GetFileNameByPath(originFileUrl);

            var urls = Excute(originFileUrl, tagName);
            if (urls.Count == 0)
            {
                MessageBox.Show("提取失败");
                return;
            }
            SaveFile(tagName, this.textBox2.Text, urls);
            MessageBox.Show($"执行结束，一共提取{urls.Count}条数据");
        }


        /// <summary>
        /// https://www.cnblogs.com/mooncher/p/3461603.html urlset 带了自定义的命名空间，需要加命名空间
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private List<string> Excute(string path,string tag)
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
                if (!string.IsNullOrWhiteSpace(url) && CheckLinkUrl(url))
                {
                    url = $"source:{url} {tag}";
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


        private void SaveFile(string name,string outDir,List<string> urls)
        {
            var fileName = $"{name}_{DateTimeOffset.Now.ToUnixTimeSeconds()}.txt";
            var path = Path.Combine(outDir, fileName);
            File.WriteAllLines(path, urls);
        }

        private bool CheckLinkUrl(string link)
        {
            if (string.IsNullOrWhiteSpace(link))
            {
                return false;
            }

            var copyLink = link.ToLower();
            var excludeFolderPaths = new string[] { "/api/","/js/", "/css/", "/images/",
                "/img/", "/visitcount/visit.jsp","/visitcount/articlehits.jsp",
                "/index.htm?aspxerrorpath"};
            var hasExcludePath = excludeFolderPaths.Any(e => copyLink.Contains(e));
            if (hasExcludePath) return false;

            var staticFileExt = ".ashx|.css|.jpg|.jpeg|.png|.gif|.bmp|.pdf|.rar|.zip|.doc|.docx|.wps|.ppt|.pptx|.txt|.rtf|.md|.xls|.xlsx|.mp4|.avi|.ogv|.webm|.flv|.f4v|.wmv|.mp3|.ogg|.wav|.wma|.mid|.svg|.webp";
            var staticFileExtArr = staticFileExt.Split("|").ToList();

           

            var hasStaticFile = staticFileExtArr.Any(e => copyLink.Contains(e));

            if (hasStaticFile) return false;
            else
            {
                if (!copyLink.Contains(".jsp") && copyLink.Contains(".js"))  // 区分 .js
                {
                    return false;
                }
            }
            return true;
        }

        private string GetFileNameByPath(string path)
        {
            var fileInfo = new FileInfo(path);
            var arr = fileInfo.Name.Split(".");
            return arr.FirstOrDefault() ?? "";
        }
    }
}
