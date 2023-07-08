// PolyFill.cs - Used to fill polygon
// ------------------------------------------------------------------------------------------------

namespace GrayBMP;

#region class PolyFill ----------------------------------------------------------------------------
class PolyFill {
   #region Methods --------------------------------------------------
   /// <summary>Add a line to make polygon given start and end point</summary>
   public void AddLine (int x0, int y0, int x1, int y1)
      => mPoints.AddRange (new[] { x0, y0, x1, y1 });

   /// <summary>After adding all the lines for polygon, fill it</summary>
   public void Fill (GrayBMP bmp, int color) {
      // Fill the polygon
      for (int i = 0; i < bmp.Height; i++) {
         int cnt = mPoints.Count;
         List<int> intersections = new ();
         for (int j = 0; j < cnt; j += 4) {
            int x = GetIntersection (j, i);
            if (int.MaxValue != x) intersections.Add (x);
         }
         if (intersections.Count > 0)
            bmp.DrawHorizontalLines (i, color, intersections.OrderBy (a => a).ToArray ());
      }
   }
   #endregion

   #region Implementation -------------------------------------------
   // Get the intersection of line represented by idx (take four point in X, Y order)
   // with horizontal line
   int GetIntersection (int idx, int scanY) {
      int x1 = mPoints[idx], y1 = mPoints[idx + 1], x2 = mPoints[idx + 2], y2 = mPoints[idx + 3];
      // Check if scanY is in the range
      int minY = y1, maxY = y2;
      if (y1 > y2) (minY, maxY) = (y2, y1);
      double dScanY = scanY + 0.5;
      if (dScanY < minY || dScanY > maxY) return int.MaxValue;
      int dx = x2 - x1;
      // Vartical line
      if (dx == 0) return x2;
      // Horizontal line
      int dy = y2 - y1;
      if (dy == 0) return int.MaxValue;
      // Line Equation y = mx + c
      double m = (double)dy / dx, c = y1 - m * x1;
      int x = (int)((dScanY - c) / m);
      int minX = x1, maxX = x2;
      if (x1 > x2) (minX, maxX) = (x2, x1);
      if (x < minX || x > maxX) return int.MaxValue;
      return x;
   }
   #endregion

   #region Private Data ---------------------------------------------
   internal List<int> mPoints = new ();
   #endregion
}
#endregion

