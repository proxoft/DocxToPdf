using System.Drawing;
using Proxoft.DocxToPdf.Models.Common;

namespace Proxoft.DocxToPdf.Models.Tables.Elements
{
    internal class TableBorderStyle : BorderStyle
    {
        private static readonly Pen _defaultPen = new Pen(Color.Black, 4.EpToPoint());

        public static readonly TableBorderStyle Default = new TableBorderStyle(_defaultPen);

        public TableBorderStyle(Pen all) : this(all, all, all, all, all, all)
        {
        }

        public TableBorderStyle(Pen top, Pen right, Pen bottom, Pen left, Pen insideHorizontal, Pen insideVertical) : base(top, right, bottom, left)
        {
            this.InsideHorizontal = insideHorizontal;
            this.InsideVertical = insideVertical;
        }

        public Pen InsideHorizontal { get; }
        public Pen InsideVertical { get; }
    }
}
