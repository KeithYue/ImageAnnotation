using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using Chilkat;
using FlickrNet;

using ExifLibrary;


namespace ImageAnnotation_beta1_0
{
    //图片上传方式
    public enum PhotoLocation:int
    {
        LOCAL=0,
        WEB=1,
        Uknow
    }

    //在mainwindow上显示的图像类
    public class MyPhoto:INotifyPropertyChanged
    {
        public MyPhoto()
        {

        }
        public MyPhoto(PhotoLocation location, string uri)
        {
            this.Location = location;

            this.Uri = uri;
        }

        public void initialize()
        {
            this.MetaData = new Dictionary<string,string>();
            this.BitImg = null;
            this.Tags = null;
            this.Location = PhotoLocation.Uknow;
            this.Uri = null;
            if (this.MySearchHelper == null)
            {
                this.MySearchHelper = new SearchPhotoHelper();
                this.MySearchHelper.initialize();
            
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private List<string> generateTags(string imgPath , PhotoLocation type)
        {
            List<string> tags = new List<string>();
            Http http = new Http();
            bool success;
            string html;//the html body to be analyse
            success = http.UnlockComponent("30277129240");//进行组件验证

            if (success != true)
            {
                MessageBox.Show(http.LastErrorText);
                return null;
            }

            HttpRequest req = new HttpRequest();
            string domain = "http://alipr.com";
            int port;
            bool ssl;
            domain = "alipr.com";
            port = 80;
            ssl = false;
            switch (type)
            {
                case PhotoLocation.WEB:
                    req.UsePost();
                    req.AddParam("nimg", "15");
                    req.AddParam("image", imgPath);
                    req.Path = "/cgi-bin/alipr.cgi";
                    break;
                case PhotoLocation.LOCAL:
                    req.UseUpload();
                    req.Path = "/cgi-bin/alipr_upload.cgi";
                    req.AddFileForUpload("upfile", imgPath);
                    break;
                default:
                    return null;
            }
            Chilkat.HttpResponse resp = null;//响应信息
            resp = http.SynchronousRequest(domain, port, ssl, req);
            html = resp.BodyStr;
            HtmlUtil htmlUtil = new HtmlUtil();
            html = htmlUtil.EntityDecode(html);

            HtmlToXml htmlToXml = new HtmlToXml();
            success = htmlToXml.UnlockComponent("HtmlToXmlT34MB34N_320C4A82mR5H");
            if (success != true)
            {
                MessageBox.Show(htmlToXml.LastErrorText);
                return null;
            }

            string xhtml;
            htmlToXml.Html = html;
            htmlToXml.DropTextFormattingTags();
            xhtml = htmlToXml.ToXml();

            Xml xml = new Xml();
            xml.LoadXml(xhtml);

            //xml遍历 获得标签
            Xml label;
            Xml xBeginSearchAfter = null;
            label = xml.SearchForTag(xBeginSearchAfter, "label");
            while (!(label == null))
            {
                tags.Add(label.AccumulateTagContent("text", "input"));
                xBeginSearchAfter = label;
                label = xml.SearchForTag(xBeginSearchAfter, "label");

            }

            return tags;

        }

        public void getTags()
        {
            Tags = this.generateTags(this.Uri, this.Location);
        }

        public List<Photo> similarPhotos;

        private List<string> tags;

        //private EXIFextractor metaData;
        private Dictionary<string ,string >  metaData;

        public Dictionary<string,string> MetaData
        {
            get
            {
                return metaData;
            }
            set
            {
                metaData = value;
                if (this.PropertyChanged != null)
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("MetaData"));
            }
        }

        


        public List<string> Tags
        {
            get
            {
                    return tags;
            }
            set
            {
                tags = value;
                if (this.PropertyChanged != null)
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Tags"));
            }
        }

        private PhotoLocation location;

        public PhotoLocation Location
        {
            get
            {
                return location;
            }
            set
            {
                location = value;
                if (this.PropertyChanged != null)
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Location"));
            }
        }

        private string uri;

        public string Uri
        {
            get
            {
                return uri;
            }
            set
            {
                uri = value;
                if (this.PropertyChanged != null)
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Uri"));
            }
        }

        private BitmapImage bitImg;

        public BitmapImage BitImg
        {
            get
            {
                return bitImg;
            }
            set
            {
                bitImg = value;
                if (this.PropertyChanged != null)
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("BitImg"));
            }
        }
        private SearchPhotoHelper mySearchHelper;

        public SearchPhotoHelper MySearchHelper
        {
            get
            {
                return mySearchHelper;
            }
            set
            {
                mySearchHelper = value;
            }
        }

        public PhotoCollection getSimiliarPhotos(PhotoSearchOptions opts,int index = 1 )
        {
            PhotoCollection p;

            string tags = "";

            foreach (string s in this.MySearchHelper.MySearchTags)
            {
                tags += (s + ",");
            }
            tags = tags.Substring(0, tags.Length - 1);
            if ((this.MySearchHelper.MyImgSource.Name == "Flickr"))
            {
                Flickr Flr = new Flickr("2fb2f26a8c5837d27e28cfeb6ebace00", "064c36b3ec29d9d2");

                //MessageBox.Show(tags);
                opts.Tags = tags;
                opts.Page = index;
                p = Flr.PhotosSearch(opts);
                //string result = "";
                return p;
                //MessageBox.Show(result);

            }
            else
            {
                return null;
            }
        }
        
        
    }

    public class ImageSource
    {
        public ImageSource()
        {
        }

        public ImageSource(string name, BitmapImage icon, string site)
        {
            this.Name = name;
            this.Icon = icon;
            this.Site = site;
        }
        public string Name
        {
            get;
            set;
        }

        public BitmapImage Icon
        {
            get;
            set;
        }

        public string Site
        {
            get;
            set;
        }
    }

    public class SearchMethod
    {
        public string Name
        {
            get;
            set;
        }
       
    }

    public class SearchPhotoHelper
    {
        public SearchPhotoHelper()
        {
        }

        public void initialize()
        {
            this.MySearchMethod = null;
            this.MyImgSource = null;
            this.MySearchTags = new List<string>();
        }
        public ImageSource MyImgSource
        {
            get;
            set;
        }

        public SearchMethod MySearchMethod
        {
            get;
            set;
        }

        public List<string> MySearchTags
        {
            get;
            set;
        }

    }
}
