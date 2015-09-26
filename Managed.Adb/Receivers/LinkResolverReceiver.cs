namespace Managed.Adb
{
    using System;
    using System.Linq;

    internal sealed class LinkResolverReceiver : MultiLineReceiver
    {
        public LinkResolverReceiver(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("fileName");
            }

            this.FileName = fileName;
            this.ParsesErrors = true;
        }

        public string FileName
        {
            get;
            set;
        }

        public string ResolvedPath
        {
            get;
            set;
        }

        protected override void ProcessNewLines(string[] lines)
        {
            // all we care about is a line with '->'
            var symlinkLinks = from line in lines where line.Contains(" -> ") select line;

            foreach (var line in symlinkLinks)
            {
                string[] parts = line.Split(new string[] { " -> " }, StringSplitOptions.None);
                int fileNameIndex = parts[0].LastIndexOf(' ');

                string fileName = parts[0].Substring(fileNameIndex).Trim();
                string target = parts[1];

                if (string.Equals(this.FileName, fileName))
                {
                    this.ResolvedPath = target;
                    return;
                }
            }
        }
    }
}
