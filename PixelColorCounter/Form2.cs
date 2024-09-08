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
        private Bitmap OriginalImg { get; set; }
        private Bitmap CurrentImg { get; set; }
        private Color? ColorToHighlight { get; set; }
        private bool HighlightInSpecificColor { get; set; }
        private Color HighlightColor { get; set; }
        private bool AddGrid { get; set; }
        private bool GridDrawn { get; set; }
        private int GridXSize { get; set; }
        private int GridYSize { get; set; }
        private int GridXOffset { get; set; }
        private int GridYOffset { get; set; }

        /// <summary>
        /// Initialize the image form
        /// </summary>
        /// <param name="imagePath">path to the image the form shows</param>
        /// <param name="startX">starting X position for the form</param>
        /// <param name="startY">starting Y position for the form</param>
        /// <param name="highlightInSpecificColor">whether or not hightling a color uses the color from the color picker</param>
        /// <param name="highlightColor">color to highlight with if doing so</param>
        /// <param name="addGrid">whether or not to overlay a grid</param>
        /// <param name="gridXSize">width of the grid to add</param>
        /// <param name="gridYSize">height of the grid to add</param>
        /// <param name="gridXOffset">amount to offset the grids width</param>
        /// <param name="gridYOffset">amount to offset the grids height</param>
        public Form2(string imagePath, int startX, int startY, bool highlightInSpecificColor, Color highlightColor, bool addGrid, int gridXSize, int gridYSize, int  gridXOffset, int gridYOffset)
        {
            ImagePath = imagePath;

            ZoomLevel = 1; //default to no zoom
            ColorToHighlight = null;
            HighlightInSpecificColor = highlightInSpecificColor;
            HighlightColor = highlightColor;
            AddGrid = addGrid;
            GridDrawn = false;
            GridXSize = gridXSize;
            GridYSize = gridYSize;
            GridXOffset = gridXOffset;
            GridYOffset = gridYOffset;

            if (!string.IsNullOrEmpty(ImagePath))
            {
                OriginalImg = new(ImagePath);
                if (AddGrid)
                {
                    GridDrawn = true;
                    using Bitmap grid = AddGridToImage(OriginalImg);
                    SetCurrentImage(grid);
                }
                else
                {
                    SetCurrentImage(OriginalImg);
                }
            }

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

            if (AddGrid && !GridDrawn)
            {
                GridDrawn = true;
                using Bitmap grid = AddGridToImage(OriginalImg);
                SetCurrentImage(grid);
                ResizeToImage();
            }
            //kinda hacky way to allow the user to resize the image if a grid is drawn
            else if (AddGrid && GridDrawn)
            {
                using Bitmap grid = AddGridToImage(OriginalImg);
                SetCurrentImage(grid);
            }
            else
            {
                GridDrawn = false;
                SetCurrentImage(OriginalImg);
            }

            if (ColorToHighlight != null)
            {
                using Bitmap highlightedImage = GrayScaleHighlight(CurrentImg);
                DrawImage(graphics, highlightedImage);
            }
            else
            {
                DrawImage(graphics, CurrentImg);
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
            var width = CurrentImg.Width * ZoomLevel;
            var height = CurrentImg.Height * ZoomLevel;

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
        private void TrackBar1_ValueChanged(object sender, EventArgs e)
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
                var pixel = CurrentImg.GetPixel(e.X / ZoomLevel, e.Y / ZoomLevel);

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
        public void HighlightPixels(Color color)
        {
            ColorToHighlight = (ColorToHighlight != null && ColorToHighlight == color)
                ? null
                : color; 

            pictureBox1.Refresh();
        }

        /// <summary>
        /// If set, highlights the color to highlight in red
        /// </summary>
        /// <param name="useSpecificColor">whether or not to highlight in the selected color</param>
        public void SetHighlightType(bool useSpecificColor)
        {
            HighlightInSpecificColor = useSpecificColor;

            pictureBox1.Refresh();
        }

        /// <summary>
        /// Gets a Bitmap of the Image grayscaled except for any highlighted color
        /// Grayscale code from here: https://stackoverflow.com/questions/2265910/convert-an-image-to-grayscale
        /// </summary>
        /// <param name="startingImage">Image to grayscale highlight</param>
        /// <returns>Grayscaled Bitmap of the iamge, or null if no highlight is selected</returns>
        private Bitmap GrayScaleHighlight(Bitmap startingImage)
        {
            if (!ColorToHighlight.HasValue)
            {
                return null;
            }
            
            Bitmap highlightedImage = new(startingImage.Width, startingImage.Height);

            // Loop through the images pixels to reset color.
            for (int x = 0; x < startingImage.Width; x++)
            {
                for (int y = 0; y < startingImage.Height; y++)
                {
                    //get the pixel from the original image
                    Color originalColor = startingImage.GetPixel(x, y);

                    //ignore transparent pixels
                    if (originalColor.A != 0)
                    {
                        //don't grayscale the color to highlight
                        if (originalColor == ColorToHighlight)
                        {
                            if (HighlightInSpecificColor)
                            {
                                highlightedImage.SetPixel(x, y, HighlightColor);
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
        /// Gets a Bitmap of the Image with a grid added
        /// </summary>
        /// <param name="startingImage">image to add the grid to</param>
        /// <returns>Image with a grid over it</returns>
        private Bitmap AddGridToImage(Bitmap startingImage)
        {
            var width = startingImage.Width + ((startingImage.Width + (GridXOffset % GridXSize)) / GridXSize);
            var height = startingImage.Height + ((startingImage.Height + (GridYOffset % GridYSize)) / GridYSize);

            Bitmap imageWithGrid = new(width, height);

            for (int x = 0; x < startingImage.Width; x++)
            {
                var xGridOffset = (x + GridXOffset) / GridXSize;
                for (int y = 0; y < startingImage.Height; y++)
                {
                    var yGridOffset = (y + GridYOffset) / GridYSize;

                    //get the pixel from the original image
                    Color originalColor = startingImage.GetPixel(x, y);

                    imageWithGrid.SetPixel(x + xGridOffset, y + yGridOffset, originalColor);
                }
            }

            return imageWithGrid;
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

        /// <summary>
        /// Highlights the color of the image that was clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                var pixel = CurrentImg.GetPixel(e.X / ZoomLevel, e.Y / ZoomLevel);

                //only highlight non transparent pixels
                if (pixel.A > 0)
                {
                    HighlightPixels(pixel);
                }
            }
            catch //ignore any exceptions here
            {
            }
        }

        /// <summary>
        /// Sets the color to use in highlighting
        /// </summary>
        /// <param name="colorToUse">Color to use for highlighting</param>
        public void SetHighlightColor(Color colorToUse)
        {
            HighlightColor = colorToUse;

            pictureBox1.Refresh();
        }

        /// <summary>
        /// Sets whether or not to use a grid
        /// </summary>
        /// <param name="addGrid">true for grid, false for no grid</param>
        public void SetGrid(bool addGrid)
        {
            AddGrid = addGrid;
            GridDrawn = false;

            pictureBox1.Refresh();
        }

        /// <summary>
        /// Sets the width of the grid
        /// </summary>
        /// <param name="gridXSize">width to use for the grid</param>
        public void SetGridXSize(int gridXSize)
        {
            GridXSize = gridXSize;
            GridDrawn = false;

            pictureBox1.Refresh();
        }

        /// <summary>
        /// Sets the height of the grid
        /// </summary>
        /// <param name="gridYSize">height to use for the grid</param>
        public void SetGridYSize(int gridYSize)
        {
            GridYSize = gridYSize;
            GridDrawn = false;

            pictureBox1.Refresh();
        }

        /// <summary>
        /// Sets the width offset for the grid
        /// </summary>
        /// <param name="gridXSize">amount to offset the width of the grid</param>
        public void SetGridXOffset(int gridXSize)
        {
            GridXOffset = gridXSize;
            GridDrawn = false;

            pictureBox1.Refresh();
        }

        /// <summary>
        /// Sets the height offset for the grid
        /// </summary>
        /// <param name="gridYSize">amount to offset the height of the grid</param>
        public void SetGridYOffset(int gridYSize)
        {
            GridYOffset = gridYSize;
            GridDrawn = false;

            pictureBox1.Refresh();
        }

        /// <summary>
        /// Updates the current image and properly disposes of the old one
        /// </summary>
        /// <param name="newImage">image to use for the current image</param>
        private void SetCurrentImage(Bitmap newImage)
        {
            if (CurrentImg != null)
            {
                CurrentImg.Dispose();
            }

            CurrentImg = new(newImage);
        }
    }
}
