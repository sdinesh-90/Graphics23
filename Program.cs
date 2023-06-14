using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using static System.Math;

namespace A25;

class MyWindow : Window {
   public MyWindow () {
      Width = 800; Height = 600;
      Left = 50; Top = 50;
      WindowStyle = WindowStyle.None;
      Image image = new Image () {
         Stretch = Stretch.None,
         HorizontalAlignment = HorizontalAlignment.Left,
         VerticalAlignment = VerticalAlignment.Top,
      };
      RenderOptions.SetBitmapScalingMode (image, BitmapScalingMode.NearestNeighbor);
      RenderOptions.SetEdgeMode (image, EdgeMode.Aliased);

      mBmp = new WriteableBitmap ((int)Width, (int)Height,
         96, 96, PixelFormats.Gray8, null);
      mStride = mBmp.BackBufferStride;
      image.Source = mBmp;
      Content = image;
      MouseDown += OnMouseDown;
      DrawLine (100, 100, 200, 200);
      DrawLine (100, 200, 200, 100);
      DrawLine (100, 100, 200, 100);
      DrawLine (200, 100, 200, 200);
      DrawLine (200, 200, 100, 200);
      DrawLine (100, 200, 100, 100);
      DrawLine (0, 100, (int)Width - 1, 101);
      DrawLine (100, 0, 101, (int)Height -1);
   }

   void OnMouseDown (object sender, MouseButtonEventArgs e) {
      Point pt = e.GetPosition (this);
      if (mDrawLine) DrawLine ((int)mLastClicked.X, (int)mLastClicked.Y, (int)pt.X, (int)pt.Y);
      else mLastClicked = pt;
      mDrawLine = !mDrawLine;
   }
   Point mLastClicked;
   bool mDrawLine;

   void DrawLine (int x1, int y1, int x2, int y2) {
      // Get the line equation ax + by + c = 0
      int a = y1 - y2, b = x2 - x1, c = x1 * y2 - x2 * y1;
      var rect = new Int32Rect (Min (x1, x2), Min (y1, y2),
                                Abs (x1 - x2) + 1, Abs (y1 - y2) + 1);
      try {
         mBmp.Lock ();
         mBase = mBmp.BackBuffer;
         // In first pass, pick the best pixel in vertical direction
         for (int i = rect.X; i < rect.Width + rect.X; i++) {
            (int x, int y, _) = Enumerable.Range (rect.Y, rect.Height)
                                          .Select (y => (i, y, Err: Err (i, y)))
                                          .MinBy (a => a.Err);
            SetPixel (x, y, 255);
         }
         // In second pass, pick the best pixel in horizontal direction
         for (int j = rect.Y; j < rect.Height + rect.Y; j++) {
            (int x, int y, _) = Enumerable.Range (rect.X, rect.Width)
                                          .Select (x => (x, j, Err: Err (x, j)))
                                          .MinBy (a => a.Err);
            SetPixel (x, y, 255);
         }
         mBmp.AddDirtyRect (rect);
      } finally {
         mBmp.Unlock ();
      }

      int Err (int x, int y) => Abs (a * x + b * y + c);
   }

   void DrawLine2 (int x1, int y1, int x2, int y2) {
      // Get the line equation y = mx + c;
      double m = (double)(y2 - y1) / (x2 - x1), c = y1 - m * x1;
      double mx = (double)(x2 - x1) / (y2 - y1), cx = x1 - mx * y1;

      var rect = new Int32Rect (Min (x1, x2), Min (y1, y2),
                                Abs (x1 - x2) + 1, Abs (y1 - y2) + 1);
      try {
         mBmp.Lock ();
         mBase = mBmp.BackBuffer;
         if (!double.IsInfinity (m))
            for (int i = rect.X; i < rect.Width + rect.X; i++)
               SetPixel (i, GetY (i), 255);
         if (!double.IsInfinity (mx))
            for (int j = rect.Y; j < rect.Height + rect.Y; j++)
               SetPixel (GetX (j), j, 255);
         mBmp.AddDirtyRect (rect);
      } finally {
         mBmp.Unlock ();
      }

      int GetY (int x) => (int)(m * x + c);
      int GetX (int y) => (int)(mx * y + cx);
   }

   void DrawMandelbrot (double xc, double yc, double zoom) {
      try {
         mBmp.Lock ();
         mBase = mBmp.BackBuffer;
         int dx = mBmp.PixelWidth, dy = mBmp.PixelHeight;
         double step = 2.0 / dy / zoom;
         double x1 = xc - step * dx / 2, y1 = yc + step * dy / 2;
         for (int x = 0; x < dx; x++) {
            for (int y = 0; y < dy; y++) {
               Complex c = new Complex (x1 + x * step, y1 - y * step);
               SetPixel (x, y, Escape (c));
            }
         }
         mBmp.AddDirtyRect (new Int32Rect (0, 0, dx, dy));
      } finally {
         mBmp.Unlock ();
      }
   }

   byte Escape (Complex c) {
      Complex z = Complex.Zero;
      for (int i = 1; i < 32; i++) {
         if (z.NormSq > 4) return (byte)(i * 8);
         z = z * z + c;
      }
      return 0;
   }

   void OnMouseMove (object sender, MouseEventArgs e) {
      if (e.LeftButton == MouseButtonState.Pressed) {
         try {
            mBmp.Lock ();
            mBase = mBmp.BackBuffer;
            var pt = e.GetPosition (this);
            int x = (int)pt.X, y = (int)pt.Y;
            SetPixel (x, y, 255);
            mBmp.AddDirtyRect (new Int32Rect (x, y, 1, 1));
         } finally {
            mBmp.Unlock ();
         }
      }
   }

   void DrawGraySquare () {
      try {
         mBmp.Lock ();
         mBase = mBmp.BackBuffer;
         for (int x = 0; x <= 255; x++) {
            for (int y = 0; y <= 255; y++) {
               SetPixel (x, y, (byte)x);
            }
         }
         mBmp.AddDirtyRect (new Int32Rect (0, 0, 256, 256));
      } finally {
         mBmp.Unlock ();
      }
   }

   void SetPixel (int x, int y, byte gray) {
      unsafe {
         var ptr = (byte*)(mBase + y * mStride + x);
         *ptr = gray;
      }
   }

   WriteableBitmap mBmp;
   int mStride;
   nint mBase;
}

internal class Program {
   [STAThread]
   static void Main (string[] args) {
      Window w = new MyWindow ();
      w.Show ();
      Application app = new Application ();
      app.Run ();
   }
}
