﻿namespace Proxoft.DocxToPdf.Models.Common
{
    internal class HorizontalSpace
    {
        public HorizontalSpace(double x, double width)
        {
            this.X = x;
            this.Width = width;
        }

        public double X { get; }
        public double Width { get; }

        public double RightX => this.X + this.Width;
    }
}
