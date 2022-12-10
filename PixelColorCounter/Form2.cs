using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PixelColorCounter
{
    public partial class Form2 : Form
    {
        //constants for max and min form size
        private const int MinWidth = 250;
        private const int MaxWidth = 800;
        private const int MinHeight = 300;
        private const int MaxHeight = 800;

        //properties used in resizing
        private readonly int DiffWidth;
        private readonly int DiffHeight;
        
        private string ImagePath { get; set; } 
        private int ZoomLevel { get; set; }
        private Bitmap Img { get; set; }

        /// <summary>
        /// Initialize the image form
        /// </summary>
        /// <param name="imagePath">path to the image the form shows</param>
        public Form2(string imagePath)
        {
            ImagePath = imagePath;
            if (!string.IsNullOrEmpty(ImagePath))
            {
                Img = new(ImagePath);
            }
            ZoomLevel = 1; //default to no zoom
            InitializeComponent();

            //get the difference in size between the form and the picture box
            DiffWidth = this.Width - pictureBox1.Width;
            DiffHeight = this.Height - pictureBox1.Height;
        }

        /// <summary>
        /// Draw the Picture
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PictureBox1_Paint(object sender, PaintEventArgs e)
        {
            var graphics = e.Graphics;

            if (ZoomLevel > 1)
            {
                Size newSize = new(Img.Width * ZoomLevel, Img.Height * ZoomLevel);
                using Bitmap zoomedImg = new(Img, newSize);

                graphics.SmoothingMode = SmoothingMode.None;
                graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.DrawImage(Img, 0, 0, zoomedImg.Width, zoomedImg.Height);
            }
            else
            {
                graphics.DrawImage(Img, 0, 0, Img.Width, Img.Height);
            }
        }

        /// <summary>
        /// Resize the form and picturebox to the image based on Zoom level and max size
        /// </summary>
        private void ResizeToImage()
        {
            var width = Img.Width * ZoomLevel;
            var height = Img.Height * ZoomLevel;

            //update the picture box size to the image size
            pictureBox1.Size = new(width, height);

            //calculate the new form width and height
            width = Math.Max(Math.Min(width + DiffWidth, MaxWidth), MinWidth);
            height = Math.Max(Math.Min(height + DiffHeight, MaxHeight), MinHeight);

            this.Size = new(width, height);
        }

        /// <summary>
        /// Update the image Zoom level and resize
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TrackBar1_ValueChanged(object sender, System.EventArgs e)
        {
            ZoomLevel = (trackBar1.Value == 0) ? 1 : (int)Math.Pow(2, trackBar1.Value);
            label1.Text = $"x{ZoomLevel}";

            ResizeToImage();

            pictureBox1.Refresh();
        }

        /// <summary>
        /// Show the image form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form2_Shown(object sender, EventArgs e)
        {
            ResizeToImage();

            pictureBox1.Refresh();
        }

        /// <summary>
        /// Redraw on manual resize of the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form2_Resize(object sender, EventArgs e)
        {
            pictureBox1.Refresh();
        }

        /// <summary>
        /// Display tooltip with the color and RGB value of the color at a certain pixel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                var pixel = Img.GetPixel(e.X / ZoomLevel, e.Y / ZoomLevel);
                if (pixel.A > 0)
                {
                    toolTip1.BackColor = pixel;
                    //got this calculation for determine white or black text from: https://stackoverflow.com/questions/3942878/how-to-decide-font-color-in-white-or-black-depending-on-background-color
                    toolTip1.ForeColor = (pixel.R * 0.299 + pixel.G * 0.587 + pixel.B * 0.114 > 186)
                        ? Color.Black
                        : Color.White;
                    toolTip1.SetToolTip(pictureBox1, $"R:{pixel.R}, G:{pixel.G}, B:{pixel.B}");
                }
                else //don't display tooltip for transparent pixels
                {
                    toolTip1.BackColor = SystemColors.Info;
                    toolTip1.ForeColor = SystemColors.InfoText;
                    toolTip1.SetToolTip(pictureBox1, null);
                }
            }
            catch //ignore any exceptions here and just remove the tooltip
            {
                toolTip1.BackColor = SystemColors.Info;
                toolTip1.ForeColor = SystemColors.InfoText;
                toolTip1.SetToolTip(pictureBox1, null);
            }
        }

        /// <summary>
        /// Custom draw for the tooltip
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToolTip1_Draw(object sender, DrawToolTipEventArgs e)
        {
            e.DrawBackground();
            e.DrawBorder();
            e.DrawText();
        }
    }
}
