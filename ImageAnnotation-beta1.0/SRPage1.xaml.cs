using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Net;
using System.Diagnostics;
using System.ComponentModel;
using Chilkat;
using FlickrNet;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;
using System.Windows.Navigation;


namespace ImageAnnotation_beta1_0
{
	public partial class SRPage1
	{
        public PhotoData pd;

        public PhotoSearchOptions po;

        public static Photo SPhoto=new Photo();

		public SRPage1()
		{
            this.InitializeComponent();

            this.Loaded += OnLoadCompleted;

            this.pd = new PhotoData();

            pd.initialize();

            this.po = new PhotoSearchOptions();

            this.po.Extras = PhotoSearchExtras.All;

            //为图片展示绑定数据
            this.Pl1.ItemsSource = this.pd.p1;
            this.Pl2.ItemsSource = this.pd.p2;
            this.Pl3.ItemsSource = this.pd.p3;

 		}

        public void OnLoadCompleted(object sender, EventArgs e)
        {
            MyPhoto mp = ((MainWindow)Application.Current.MainWindow).MainPhoto;

            PhotoCollection result1 = mp.getSimiliarPhotos(this.po);

            fullFillPData(result1);
            
        }

        private void fullFillPData(PhotoCollection p)
        {
            if (p != null)
            {
                foreach(Photo i in p)
                {
                    //调试用
                    Console.Write(i.OriginalUrl);
                    int h1 = this.pd.h1;
                    int h2 = this.pd.h2;
                    int h3 = this.pd.h3;
                    if (h1 <= h2 && h1 <= h3)
                    {
                        this.pd.p1.Add(i);
                        this.pd.h1 += (int)(i.SmallHeight*216/i.SmallWidth);
                    }
                    else if (h2 <= h1 && h2 <= h3)
                    {
                        this.pd.p2.Add(i);
                        this.pd.h2 += (int)(i.SmallHeight * 216 / i.SmallWidth);
                    }
                    else if (h3 <= h1 && h3 <= h2)
                    {
                        this.pd.p3.Add(i);
                        this.pd.h3 += (int)(i.SmallHeight * 216 / i.SmallWidth);
                    }
                    this.Pl1.Height = this.Pl2.Height = this.Pl3.Height = maxThree(this.pd.h1, this.pd.h2, this.pd.h3)+10;//算上margin
                    this.Pgrid.Height = this.Pl1.Height + 2;

                    Console.WriteLine(pd.h1 + " " + pd.h2 + " " + pd.h3);
                    
                }
                
                
            }

            return;
        }

        private int maxThree(int a, int b, int c)
        {
            if (a >= b && a >= c)
                return a;
            else if (b >= a && b >= c)
                return b;
            else
                return c;
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Photo SPhoto = (Photo)((Image)e.Source).DataContext;

            //downloadSelectedPhoto(SPhoto.OriginalUrl);//下载至本地文件

        }

        //速度较慢而且不是异步
        private void downloadSelectedPhoto(string url)
        {
            Chilkat.Http http = new Chilkat.Http();

            bool success;

            //  Any string unlocks the component for the 1st 30-days.
            success = http.UnlockComponent("30277129240");
            if (success != true)
            {
                MessageBox.Show(http.LastErrorText);
                return;
            }

            //  Download the Python language install.
            //  Note: This URL may have changed since this example was created.
            success = http.Download(url,"temp.jpg");
            if (success != true)
            {
                MessageBox.Show(http.LastErrorText);
            }
            else
            {
                MessageBox.Show("Python Download Complete!");
            }

        }

        private void PhotoContext_Opened(object sender, RoutedEventArgs e)
        {

        }

        private void PhotoContext_Closed(object sender, RoutedEventArgs e)
        {

        }

        private void EditItem_Click(object sender, RoutedEventArgs e)
        {
            //Photo SPhoto = 
            //MessageBox.Show(SPhoto.OriginalUrl);

            WebClient downloader = new WebClient();

            downloader.DownloadProgressChanged += (s, de) =>
                {
                    showDownloadBar(true);

                    DownloadBar.Value = de.ProgressPercentage;

                    DownloadingText.Text = "Photo Downloading " + de.ProgressPercentage + "% ...";
                };

            downloader.DownloadFileCompleted += (s, de) =>
                {
                    DownloadingText.Text = "Photo Download Complete .";
                    showDownloadBar(false);
                    //打开美图秀秀。启动新进程

                    Process Meitu = new Process();

                    Meitu.StartInfo.FileName = @"E:\Program Files\Meitu\KanKan\KanKan.exe";

                    Meitu.StartInfo.Arguments = @"D:\C#Programming\ImageAnnotation-beta1.0\ImageAnnotation-beta1.0\bin\Debug\temp.jpg";

                    Meitu.Start();

                };

            string downloaderurl = "";

            if (SPhoto.LargeUrl!=null)
            {
                downloaderurl = SPhoto.LargeUrl;
            }
            else if (SPhoto.OriginalUrl != null)
            {
                downloaderurl = SPhoto.OriginalUrl;
            }
            else if (SPhoto.MediumUrl != null)
            {
                downloaderurl = SPhoto.MediumUrl;
            }
            else if (SPhoto.SmallUrl != null)
            {
                downloaderurl = SPhoto.SmallUrl;
            }
            else
            {
                downloaderurl = SPhoto.ThumbnailUrl;
            }

            if (downloaderurl != "")
            {
                //MessageBox.Show(downloaderurl);
                downloader.DownloadFileAsync(new Uri(downloaderurl), "temp.jpg");
            }
            
            
        }

        private void Image_MouseEnter(object sender, MouseEventArgs e)
        {
            SPhoto = (Photo)((Image)sender).DataContext;
        }

        private void Image_MouseLeave(object sender, MouseEventArgs e)
        {
            SPhoto = null;
        }

        private void showDownloadBar(bool open)
        {
            DownloadPopup.IsOpen = open;

            SRPageCanvas.IsEnabled = !open;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {

        }


        
    }

    public class PhotoData
    {
        public PhotoCollection p1;
        public PhotoCollection p2;
        public PhotoCollection p3;
        public int h1, h2, h3;//三列的高度


        public void initialize()
        {
            if(p1==null)
                p1=new PhotoCollection();
            if(p2==null)
                p2=new PhotoCollection();
            if (p3 == null)
                p3 = new PhotoCollection();
            h1 = 0;
            h2 = 0;
            h3 = 0;
        }
    }
}