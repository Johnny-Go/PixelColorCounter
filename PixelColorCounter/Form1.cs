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
        private const int MaxHeight = 800;

        private Dictionary<Color, int> PixelColorCount { get; set; }
        private int TotalPixelCount { get; set; }

        /// <summary>
        /// initialize the form
        /// </summary>
        public Form1()
        {
            PixelColorCount = new();
            TotalPixelCount = 0;
            InitializeComponent();

            //update combobox items
            List<ComboBoxListItem> items = new()
            {
                new("Count", 0),
                new("Color", 1)
            };
            this.comboBox1.DisplayMember = "Text";
            this.comboBox1.ValueMember = "Id";
            this.comboBox1.DataSource = items;
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
                graphics.DrawString($"Total Pixels: {TotalPixelCount}\tTotalColors: {PixelColorCount.Count}", myFont, Brushes.Black, new Point(0, 5));

                int i = 1; //start at 1 to offset the Total count
                foreach (var kvp in PixelColorCount)
                {
                    Rectangle rect = new(0, i * 45, 40, 40);
                    SolidBrush brush = new(kvp.Key);
                    graphics.FillRectangle(brush, rect);

                    //only draw RGB values if requested
                    if (checkBox1.Checked)
                    {
                        graphics.DrawString($"{kvp.Value}\t\t(R:{kvp.Key.R},\tG:{kvp.Key.G},\tB:{kvp.Key.B})", myFont, Brushes.Black, new Point(45, (i * 45) + 5));
                    }
                    else
                    {
                        graphics.DrawString($"{kvp.Value}\t", myFont, Brushes.Black, new Point(45, (i * 45) + 5));
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
        /// reads the image and builds our pixel color count dictionary and total pixel count when a file is selected
        /// </summary>
        private void OpenFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
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
        }

        /// <summary>
        /// Automatically resize the form and picturebox when selecting an image
        /// </summary>
        private void AutoResize()
        {
            var height = (PixelColorCount.Count * 45) + 45;
            pictureBox1.Size = new(425 - (SystemInformation.VerticalScrollBarWidth + 4), height);

            height = (height > MaxHeight)
                ? MaxHeight
                : (height += 112);

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
    }
}
