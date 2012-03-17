using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;

using ExifLibrary;


namespace ImageAnnotation_beta1_0
{
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window ,INotifyPropertyChanged
	{
        public event PropertyChangedEventHandler PropertyChanged;

        private MyPhoto mainPhoto;

        public MyPhoto MainPhoto
        {
            get
            {
                return mainPhoto;
            }
            set
            {
                mainPhoto = value;
            }
        }

		public MainWindow()
		{
			this.InitializeComponent();

			// 在此点下面插入创建对象所需的代码。

            MainPhoto = new MyPhoto();

            MainPhoto.initialize();//进行各项参数的初始化

            //将相关控件的数据源绑定到mainPhoto

            //为Image控件绑定
            Binding imgShowBingding = new Binding();
            imgShowBingding.Source = MainPhoto;
            imgShowBingding.Path = new PropertyPath("Uri");
            this.MainImage.SetBinding(Image.SourceProperty, imgShowBingding);

            //为TagList控件绑定
            Binding tagsListBinding = new Binding();
            tagsListBinding.Source = MainPhoto;
            tagsListBinding.Path = new PropertyPath("Tags");
            this.tagsList.SetBinding(ListBox.ItemsSourceProperty, tagsListBinding);

            //为图像元数据绑定
            Binding metaBinding = new Binding();
            metaBinding.Source = MainPhoto;
            metaBinding.Path = new PropertyPath("MetaData");
            this.MetaDataView.SetBinding(ListView.ItemsSourceProperty, metaBinding);

            

            ////为getTagsButton的可见性进行绑定
            //Binding getTagsButtonBinding = new Binding();
            //getTagsButtonBinding.Source = MainPhoto;
            //getTagsButtonBinding.Path = new PropertyPath("Uri");
            //getTagsButtonBinding.Converter = new Str2VHConverter();
            //getTagButton.SetBinding(Button.VisibilityProperty, getTagsButtonBinding);

            //初始化ImageSource集合
            List<ImageSource> imgSources = new List<ImageSource>()
            {
                new ImageSource(){Name="Flickr", Icon=new BitmapImage(new Uri("D:\\C#Programming\\ImageAnnotation-beta1.0\\ImageAnnotation-beta1.0\\Icons\\flickr.png",UriKind.Absolute))},
                new ImageSource(){Name="Picasa"},
                new ImageSource(){Name="Panoramio"},
                new ImageSource(){Name="Windows Live"},
                new ImageSource(){Name="Zooomr"},
                new ImageSource(){Name="又拍"},
                new ImageSource(){Name="巴巴变"},
                new ImageSource(){Name="SmugMug"},
                new ImageSource(){Name="Photobucket"},
                new ImageSource(){Name="SmugMug"},

            };
            this.ImageSourceComboBox.ItemsSource = imgSources;
            //this.ImageSourceComboBox.DisplayMemberPath = "Name";
            //this.ImageSourceComboBox.SelectedValuePath = ".";

            //为filenametextbox添加事件,目前为双击
            this.FileNameTextbox.MouseDoubleClick += (s, e) =>
            {
                OpenFileDialog dlg = new OpenFileDialog();
                // Display OpenFileDialog by calling ShowDialog method

                Nullable<bool> result = dlg.ShowDialog();

                // Get the selected file name and display in a TextBox

                if (result == true)
                {

                    // Open document

                    string filename = dlg.FileName;

                    FileNameTextbox.Text = filename;

                }

            };

            //为getimageBUTTON添加事件
            this.GetImageButton.Click += (s, e) =>
            {
                MainPhoto.initialize();

                //this.SearchFrame.Content = this.SearchInitCanvas;
                
                if ((FileNameTextbox.Text == null) && (ImageUrl.Text == null))
                {
                    MessageBox.Show("Please Input A File");
                }

                if (this.InputImage.SelectedIndex == 0)
                {
                    ////获取图像元信息
                    //System.Drawing.Bitmap bmp=new System.Drawing.Bitmap(FileNameTextbox.Text);
                    //mainPhoto.MetaData = new Goheer.EXIF.EXIFextractor(ref bmp,"");

                    //foreach (System.Web.UI.Pair sb in mainPhoto.MetaData)
                    //{
                    //    // Remember the data is returned 
                    //    // in a Key,Value Pair object
                    //    Console.Write(sb.First + "  " + sb.Second + "\n");
                    //}
                    // Extract exif metadata

                    ExifFile file = ExifFile.Read(FileNameTextbox.Text);

                    //// Read metadata
                    foreach (ExifProperty item in file.Properties.Values)
                    {
                        //Console.WriteLine(item.Tag.ToString() + " : " + item.Value.ToString());
                        mainPhoto.MetaData.Add(item.Tag.ToString(),item.Value.ToString()); 
                    }


                    

                    mainPhoto.Location = PhotoLocation.LOCAL;
                    mainPhoto.Uri = FileNameTextbox.Text;
                    mainPhoto.BitImg = new BitmapImage(new Uri(mainPhoto.Uri));


                }

                else if (this.InputImage.SelectedIndex == 1)
                {
                    mainPhoto.Location = PhotoLocation.WEB;
                    mainPhoto.Uri = ImageUrl.Text;
                    //此处还为添加加载网络图片的代码
                }
                else
                {

                }

                //显示GETtag按钮
                this.getTagButton.Visibility = Visibility.Visible;

            };

            //为getimageTag添加事
            this.getTagButton.Click += (s, e) =>
            {
                MainPhoto.getTags();
                this.getTagButton.Visibility = Visibility.Hidden;

            };

		}

        private void TagCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = (CheckBox)sender;

            if (chk.IsChecked == true)
            {
                if (!(MainPhoto.MySearchHelper.MySearchTags.Contains((string)chk.Content)))

                    MainPhoto.MySearchHelper.MySearchTags.Add((string)chk.Content);
            }

            else
            {
                MainPhoto.MySearchHelper.MySearchTags.Remove((string)chk.Content);
            }

        }

        private void ImageSourceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = (ComboBox) sender;
            MainPhoto.MySearchHelper.MyImgSource = (ImageSource)cb.SelectedItem;
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            SRPage1 srp = new SRPage1();
            this.SearchFrame.NavigationService.Navigate(srp);

        }

        
    }

}