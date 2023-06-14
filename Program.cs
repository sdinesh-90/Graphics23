using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

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
   }

   void OnMouseDown (object sender, MouseButtonEventArgs e) {
      Point pt = e.GetPosition (this);
      if (mDrawLine) DrawLine ((int)mLastClicked.X, (int)mLastClicked.Y, (int)pt.X, (int)pt.Y);
      else mLastClicked = pt;
      mDrawLine = !mDrawLine;
   }
   Point mLastClicked;
   bool mDrawLine;

   void DrawLine2 (int x1, int y1, int x2, int y2) {
      double slope = (double)(y2 - y1) / (x2 - x1);
      Console.WriteLine (slope);
      int a = y1 - y2, b = x2 - x1, c = (x1 - x2) * y1 + (y2 - y1) * x1;
      var rect = new Int32Rect (Math.Min (x1, x2), Math.Min (y1, y2),
                                Math.Abs (x1 - x2) + 1, Math.Abs (y1 - y2) + 1);
      try {
         mBmp.Lock ();
         mBase = mBmp.BackBuffer;
         int x = rect.X, y = rect.Y;
         int trx = rect.X + rect.Width - 1, try1 = rect.Y + rect.Height - 1;
         // In first pass, pick the best pixel in vertical direction
         for (int i = 0; i < rect.Width; i++) {
            int minDist = int.MaxValue, rx = x, ry = y;
            for (int j = 0; j < rect.Height; j++) {
               int nx = i + rect.X, ny = j + rect.Y;
               int dist = Math.Abs (OnLine (nx, ny));
               if (dist < minDist) { minDist = dist; rx = nx; ry = ny; }
            }
            SetPixel (rx, ry, 255);
         }
         // In second pass, pick the best pixel in horizontal direction
         for (int i = 0; i < rect.Height; i++) {
            int minDist = int.MaxValue, rx = x, ry = y;
            for (int j = 0; j < rect.Width; j++) {
               int nx = j + rect.X, ny = i + rect.Y;
               int dist = Math.Abs (OnLine (nx, ny));
               if (dist < minDist) { minDist = dist; rx = nx; ry = ny; }
            }
            SetPixel (rx, ry, 255);
         }
         //SetPixel (x, y, 255);
         //int cnt = 0;
         //while (true) {
         //   if (x == trx && y == try1) break;
         //   Console.WriteLine ($"{x}, {y}");
         //   if (cnt > rect.Width * rect.Height) throw new NotSupportedException ();
         //   int minDist = int.MaxValue, rx = x, ry = y;
         //   for (int i = 0; i < 5; i++) {
         //      (int dx, int dy) = i switch {
         //         0 => (0, 1),
         //         1 => (1, 1),
         //         2 => (1, 0),
         //         3 => (1, -1),
         //         4 => (0, -1),
         //         _ => throw new NotImplementedException (),
         //      };
         //      int nx = x + dx, ny = y + dy;
         //      int dist = (trx - nx) * (trx - nx) + (try1 - ny) * (try1 - ny);
         //      dist = Math.Abs (OnLine (nx, ny));
         //      if (dist < minDist) { minDist = dist; rx = nx; ry = ny; }
         //   }
         //   x = rx; y = ry;
         //   SetPixel (x, y, 255);
         //}
         //for (int i = 0; i < rect.Width; i++) {
         //   for (int j = 0; j < rect.Height; j++) {
         //      int x = i + rect.X, y = j + rect.Y;
         //      if ((i == 0 && j == 0) || OnLine (x, y)) SetPixel (x, y, 255);
         //   }
         //}
         int dx1 = mBmp.PixelWidth, dy1 = mBmp.PixelHeight;
         var fullRect = new Int32Rect (0, 0, dx1, dy1);
         mBmp.AddDirtyRect (rect);
      } finally {
         mBmp.Unlock ();
      }

      int OnLine (int x, int y) {
         int res = a * x + b * y + c;
         return res;
      }
   }

   void DrawLine (int x1, int y1, int x2, int y2) {
      int a = y1 - y2, b = x2 - x1, c = (x1 - x2) * y1 + (y2 - y1) * x1;
      var rect = new Int32Rect (Math.Min (x1, x2), Math.Min (y1, y2),
                                Math.Abs (x1 - x2) + 1, Math.Abs (y1 - y2) + 1);
      try {
         mBmp.Lock ();
         mBase = mBmp.BackBuffer;
         // In first pass, pick the best pixel in vertical direction
         for (int i = 0; i < rect.Width; i++) {                  
            int minDist = int.MaxValue, x = rect.X, y = rect.Y;
            for (int j = 0; j < rect.Height; j++) {
               int nx = i + rect.X, ny = j + rect.Y;
               int dist = Math.Abs (OnLineErr (nx, ny));
               if (dist < minDist) { minDist = dist; x = nx; y = ny; }
            }
            SetPixel (x, y, 255);
         }
         // In second pass, pick the best pixel in horizontal direction
         for (int i = 0; i < rect.Height; i++) {
            int minDist = int.MaxValue, x = rect.X, y = rect.Y;
            for (int j = 0; j < rect.Width; j++) {
               int nx = j + rect.X, ny = i + rect.Y;
               int dist = Math.Abs (OnLineErr (nx, ny));
               if (dist < minDist) { minDist = dist; x = nx; y = ny; }
            }
            SetPixel (x, y, 255);
         }
         mBmp.AddDirtyRect (rect);
      } finally {
         mBmp.Unlock ();
      }

      int OnLineErr (int x, int y) => a * x + b * y + c;
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
