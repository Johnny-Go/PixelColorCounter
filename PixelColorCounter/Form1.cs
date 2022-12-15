using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PixelColorCounter
{
    public partial class Form1 : Form
    {
        private const int ConstWidth = 425;
        private const int MaxHeight = 800;
        private const int ColorBlockSize = 40;
        private const int ColorBlockTotalSpace = 45;
        private const int TextOffset = 5;

        //properties used in resizing
        private readonly int DiffHeight;

        private Dictionary<Color, int> PixelColorCount { get; set; }
        private int TotalPixelCount { get; set; }
        private Form2 ImageViewer { get; set; }

        /// <summary>
        /// Initialize the color count form
        /// </summary>
        public Form1()
        {
            PixelColorCount = new();
            TotalPixelCount = 0;
            ImageViewer = null;
            InitializeComponent();

            //set start position
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(400, 300);

            //update combobox items
            List<ComboBoxListItem> items = new()
            {
                new("Count", 0),
                new("Color", 1)
            };
            this.comboBox1.DisplayMember = "Text";
            this.comboBox1.ValueMember = "Id";
            this.comboBox1.DataSource = items;

            //get the difference in size between the form and the picture box
            DiffHeight = this.Height - pictureBox1.Height;
        }

        /// <summary>
        /// Draw the Pixel colors and count
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (PixelColorCount.Count > 0)
            {
                PixelColorCount = Sort();

                using Font myFont = new("Arial", 14);
                var graphics = e.Graphics;
                graphics.DrawString($"Total Pixels: {TotalPixelCount}\u00A0\u00A0\u00A0\u00A0\u00A0Total Colors: {PixelColorCount.Count}", myFont, Brushes.Black, new Point(0, TextOffset));

                int i = 1; //start at 1 to offset the Total count
                foreach (var kvp in PixelColorCount)
                {
                    Rectangle rect = new(0, i * ColorBlockTotalSpace, ColorBlockSize, ColorBlockSize);
                    SolidBrush brush = new(kvp.Key);
                    graphics.FillRectangle(brush, rect);

                    //only draw RGB values if requested
                    if (checkBox1.Checked)
                    {
                        graphics.DrawString($"{kvp.Value}\t\t(R:{kvp.Key.R},\tG:{kvp.Key.G},\tB:{kvp.Key.B})", myFont, Brushes.Black, new Point(ColorBlockTotalSpace, (i * ColorBlockTotalSpace) + TextOffset));
                    }
                    else
                    {
                        graphics.DrawString($"{kvp.Value}\t", myFont, Brushes.Black, new Point(ColorBlockTotalSpace, (i * ColorBlockTotalSpace) + TextOffset));
                    }

                    i += 1;
                }
            }
        }

        /// <summary>
        /// Sort the dictionary by color or count
        /// </summary>
        /// <returns>A sorted dictionary</returns>
        private Dictionary<Color, int> Sort()
        {
            //0 is by count
            if (comboBox1.SelectedIndex == 0)
            {
                return PixelColorCount
                    .OrderByDescending(kvp => kvp.Value)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }
            else
            {
                //found this sorting method on stackoverflow, it's not perfect but apparently sorting colors is an exercise in futility
                //https://stackoverflow.com/questions/62203098/c-sharp-how-do-i-order-a-list-of-colors-in-the-order-of-a-rainbow
                return PixelColorCount
                    .OrderBy(kvp => kvp.Key.GetHue())
                    .ThenBy(kvp => kvp.Key.R * 3 + kvp.Key.G * 2 + kvp.Key.B)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }
        }

        /// <summary>
        /// Reads the image and builds our pixel color count dictionary and total pixel count when a file is selected
        /// </summary>
        private void OpenFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            if (ImageViewer != null && ImageViewer.Visible)
            {
                ImageViewer.Close();
                ImageViewer.Dispose();
            }

            using Bitmap img = new(openFileDialog1.FileName);
            if (img != null)
            {
                PixelColorCount = new();
                TotalPixelCount = 0;

                for (int i = 0; i < img.Width; i++)
                {
                    for (int j = 0; j < img.Height; j++)
                    {
                        var pixel = img.GetPixel(i, j);

                        //skip fully transparent pixels
                        if (pixel.A != 0)
                        {
                            if (PixelColorCount.ContainsKey(pixel))
                            {
                                PixelColorCount[pixel] += 1;
                            }
                            else
                            {
                                PixelColorCount.Add(pixel, 1);
                            }

                            TotalPixelCount += 1;
                        }
                    }
                }

                AutoResize();
                pictureBox1.Refresh();
            }

            if (checkBox2.Checked)
            {
                ImageViewer = new(openFileDialog1.FileName, (this.Location.X + this.Width), this.Location.Y);
                ImageViewer.Show();
            }
        }

        /// <summary>
        /// Automatically resize the form and picturebox when selecting an image
        /// </summary>
        private void AutoResize()
        {
            var height = (PixelColorCount.Count * ColorBlockTotalSpace) + ColorBlockTotalSpace;
            pictureBox1.Size = new(ConstWidth, height);

            height = (height > MaxHeight)
                ? MaxHeight
                : (height += DiffHeight);

            this.Size = new(this.Width, height);
        }

        /// <summary>
        /// Show open file dialog when load button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        /// <summary>
        /// Redraw when changing the sort orer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            pictureBox1.Refresh();
        }

        /// <summary>
        /// Redraw when changing the show RGB checkbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            pictureBox1.Refresh();
        }

        /// <summary>
        /// Redraw on manual resize of the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Resize(object sender, EventArgs e)
        {
            pictureBox1.Refresh();
        }

        /// <summary>
        /// Call the highlight function if a color is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (ImageViewer != null)
            {
                if (XValueValid(e.X) && YValueValid(e.Y))
                {
                    //convert the picturebox to a bitmap, and grab the color that was clicked
                    using Bitmap bmp = new(ColorBlockSize, pictureBox1.Height);
                    pictureBox1.DrawToBitmap(bmp, pictureBox1.ClientRectangle);
                    var pixel = bmp.GetPixel(e.X, e.Y);
                    ImageViewer.HighlightPixels(pixel);
                }
            }
        }

        /// <summary>
        /// Check if the X value for a click is over a color block
        /// </summary>
        /// <param name="xValue">X value for a click</param>
        /// <returns>true if a color was clicked, false otherwise</returns>
        private static bool XValueValid(int xValue)
        {
            return (xValue >= 0) && (xValue < ColorBlockSize);
        }

        /// <summary>
        /// Check if the Y value for a click is over a color block
        /// </summary>
        /// <param name="yValue">Y value for a click</param>
        /// <returns>true if a color was clicked, false otherwise</returns>
        private bool YValueValid(int yValue)
        {
            //basically we loop over every possible color, then if the y value clicked is within the 40 vertical pixels for the color we say it's valid
            for (int i = 1; i <= PixelColorCount.Count; i++)
            {
                if(yValue >= (i * ColorBlockTotalSpace) && yValue < (i * ColorBlockTotalSpace + ColorBlockSize))
                {
                    return true;
                }
            }

            return false;
        }

        private void CheckBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (ImageViewer != null)
            {
                ImageViewer.HighlightPixelInRed(checkBox3.Checked);
            }
        }
    }
}
