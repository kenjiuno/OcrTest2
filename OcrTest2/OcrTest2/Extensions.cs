using Microsoft.ProjectOxford.Vision.Contract;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OcrTest2 {
    public static class Extensions {
        public static SKRect ToSKRect(this Rectangle rect) {
            return SKRect.Create(rect.Left, rect.Top, rect.Width, rect.Height);
        }
    }
}
