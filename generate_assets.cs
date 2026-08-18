using System;
using System.Drawing;
using System.Drawing.Imaging;

class AssetGenerator
{
    static void Main()
    {
        // Generate icon
        GenerateIcon();
        Console.WriteLine("Assets generated!");
    }

    static void GenerateIcon()
    {
        int size = 256;
        using var bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(bmp);
        
        // Clear transparent
        g.Clear(Color.Transparent);
        
        // Background circle
        using var bgBrush = new SolidBrush(Color.FromArgb(255, 20, 20, 30));
        g.FillEllipse(bgBrush, 10, 10, size - 20, size - 20);
        
        // Hill shape
        using var hillBrush = new SolidBrush(Color.FromArgb(255, 60, 100, 50));
        var hillPoints = new Point[]
        {
            new Point(50, size - 50),
            new Point(size / 2 - 30, size / 2 + 20),
            new Point(size / 2, size / 2 - 40),
            new Point(size / 2 + 30, size / 2 + 20),
            new Point(size - 50, size - 50)
        };
        g.FillPolygon(hillBrush, hillPoints);
        
        // Car body
        using var carBrush = new SolidBrush(Color.FromArgb(255, 200, 200, 50));
        g.FillRectangle(carBrush, size / 2 - 40, size / 2 - 20, 80, 30);
        
        // Wheels
        using var wheelBrush = new SolidBrush(Color.FromArgb(255, 30, 30, 30));
        g.FillEllipse(wheelBrush, size / 2 - 35, size / 2 + 10, 20, 20);
        g.FillEllipse(wheelBrush, size / 2 + 15, size / 2 + 10, 20, 20);
        
        // Wheel rims
        using var rimBrush = new SolidBrush(Color.FromArgb(255, 100, 100, 110));
        g.FillEllipse(rimBrush, size / 2 - 30, size / 2 + 15, 10, 10);
        g.FillEllipse(rimBrush, size / 2 + 20, size / 2 + 15, 10, 10);
        
        // "HCR" text
        using var font = new Font("Arial", 36, FontStyle.Bold);
        using var textBrush = new SolidBrush(Color.FromArgb(255, 255, 220, 50));
        g.DrawString("HCR", font, textBrush, size / 2 - 40, 30);
        
        // Save
        string outputPath = @"C:\Users\JahDaGanj\HillClimbRacingWinFork\assets\ui\icon.png";
        bmp.Save(outputPath, ImageFormat.Png);
        Console.WriteLine($"Icon saved to {outputPath}");
    }
}