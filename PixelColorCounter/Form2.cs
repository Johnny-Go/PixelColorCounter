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
        private Color? ColorToHighlight { get; set; }
        private bool HighlightInRed { get; set; }

        /// <summary>
        /// Initialize the image form
        /// </summary>
        /// <param name="imagePath">path to the image the form shows</param>
        public Form2(string imagePath, int startX, int startY)
        {
            ImagePath = imagePath;
            if (!string.IsNullOrEmpty(ImagePath))
            {
                Img = new(ImagePath);
            }
            ZoomLevel = 1; //default to no zoom
            ColorToHighlight = null;
            HighlightInRed = false;

            InitializeComponent();

            //set start position
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(startX, startY);

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
            if (ColorToHighlight != null)
            {
                using Bitmap highlightedImage = GrayScaleHighlight();
                DrawImage(graphics, highlightedImage);
            }
            else
            {
                DrawImage(graphics, Img);
            }
        }

        /// <summary>
        /// Function to do the actual drawing
        /// </summary>
        /// <param name="graphics">graphics object ot do the drawing</param>
        /// <param name="image">image to draw</param>
        private void DrawImage(Graphics graphics, Bitmap image)
        {
            var width = image.Width;
            var height = image.Height;

            //update the draw settings if the image is zoomed
            if (ZoomLevel > 1)
            {
                width = image.Width * ZoomLevel;
                height = image.Height * ZoomLevel;

                graphics.SmoothingMode = SmoothingMode.None;
                graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            }

            graphics.DrawImage(image, 0, 0, width, height);
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

                if (pixel.A > 0 && (ColorToHighlight == null || ColorToHighlight == pixel))
                {
                    toolTip1.BackColor = pixel;
                    //got this calculation for determine white or black text from: https://stackoverflow.com/questions/3942878/how-to-decide-font-color-in-white-or-black-depending-on-background-color
                    toolTip1.ForeColor = (pixel.R * 0.299 + pixel.G * 0.587 + pixel.B * 0.114 > 186)
                        ? Color.Black
                        : Color.White;
                    toolTip1.SetToolTip(pictureBox1, $"R:{pixel.R}, G:{pixel.G}, B:{pixel.B}");
                }
                else //don't display tooltip for transparent pixels, or pixels that don't match the highlighted color
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

        /// <summary>
        /// Sets the color to highlight or clears it and redraws
        /// </summary>
        /// <param name="color">Color to highlight</param>
        public void HighlightColor(Color color)
        {
            ColorToHighlight = (ColorToHighlight != null && ColorToHighlight == color)
                ? null
                : color; 

            pictureBox1.Refresh();
        }

        /// <summary>
        /// If set, highlights the color to highlight in red
        /// </summary>
        /// <param name="highlightInRed">whether or not to highlight in red</param>
        public void HighlightPixelInRed(bool highlightInRed)
        {
            HighlightInRed = highlightInRed;

            pictureBox1.Refresh();
        }

        /// <summary>
        /// Gets a Bitmap of the Image grayscaled except for any highlighted color
        /// Grayscale code from here: https://stackoverflow.com/questions/2265910/convert-an-image-to-grayscale
        /// </summary>
        /// <returns>Grayscaled Bitmap of the iamge, or null if no highlight is selected</returns>
        private Bitmap GrayScaleHighlight()
        {
            if (!ColorToHighlight.HasValue)
            {
                return null;
            }
            
            Bitmap highlightedImage = new(Img.Width, Img.Height);

            // Loop through the images pixels to reset color.
            for (int x = 0; x < Img.Width; x++)
            {
                for (int y = 0; y < Img.Height; y++)
                {
                    //get the pixel from the original image
                    Color originalColor = Img.GetPixel(x, y);

                    //ignore transparent pixels
                    if (originalColor.A != 0)
                    {
                        //don't grayscale the color to highlight
                        if (originalColor == ColorToHighlight)
                        {
                            if (HighlightInRed)
                            {
                                //set the new image's pixel to the grayscale version
                                highlightedImage.SetPixel(x, y, Color.Red);
                            }
                            else
                            {
                                highlightedImage.SetPixel(x, y, ColorToHighlight.Value);
                            }
                        }
                        else
                        {
                            //create the grayscale version of the pixel
                            int grayScale = (int)((originalColor.R * 0.3) + (originalColor.G * 0.59) + (originalColor.B * 0.11));

                            //create the color object
                            Color newColor = Color.FromArgb(grayScale, grayScale, grayScale);

                            //set the new image's pixel to the grayscale version
                            highlightedImage.SetPixel(x, y, newColor);
                        }
                    }
                }
            }

            return highlightedImage;
        }

        /// <summary>
        /// Handle Mouse Wheel on the trackbar nicely
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TrackBar1_MouseWheel(object sender, MouseEventArgs e)
        {
            ((HandledMouseEventArgs)e).Handled = true; //disable normal mouse wheel event

            if ((e.Delta > 0) && (trackBar1.Value < trackBar1.Maximum))
            {
                trackBar1.Value += 1;
            }
            else if ((e.Delta < 0) && (trackBar1.Value > trackBar1.Minimum))
            {
                trackBar1.Value -= 1;
            }
        }
    }
}
