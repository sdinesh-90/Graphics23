// PolyFill.cs - Used to fill polygon
// ------------------------------------------------------------------------------------------------

using System.Collections.Concurrent;

namespace GrayBMP;

#region class PolyFill ----------------------------------------------------------------------------
class PolyFill {
   #region MultiThread Implementation -------------------------------
   // Fill the polygon using Task. For this to work, Begin and End should be removed from
   // DrawHorizontalLines method of GrayBMP
   public void FillTask (GrayBMP bmp, int color) {
      (mBmp, mColor) = (bmp, color);
      mBmp.Begin ();
      mQueue.Clear ();
      for (int i = 0; i < bmp.Height; i++) mQueue.Enqueue (i);
      Task[] tasks = new Task[Environment.ProcessorCount];
      for (int i = 0; i < tasks.Length; i++)
         tasks[i] = Task.Run (TaskProc);
      Task.WaitAll (tasks);
      mBmp.End ();
   }

   // This is the function being executed by each of the tasks
   void TaskProc () {
      while (mQueue.TryDequeue (out int y)) Fill (y);
   }
   readonly ConcurrentQueue<int> mQueue = new ();
   #endregion

   #region Methods --------------------------------------------------
   /// <summary>Add a line to make polygon given start and end point</summary>
   public void AddLine (int x0, int y0, int x1, int y1)
      => mPoints.AddRange (new[] { x0, y0, x1, y1 });

   /// <summary>After adding all the lines for polygon, fill it</summary>
   public void Fill (GrayBMP bmp, int color) {
      (mBmp, mColor) = (bmp, color);
      bmp.Begin ();
      // Fill the polygon
      for (int i = 0; i < bmp.Height; i++) Fill (i);
      bmp.End ();
   }
   #endregion

   #region Implementation -------------------------------------------
   void Fill (int row) {
      int cnt = mPoints.Count;
      List<int> intersections = new ();
      for (int j = 0; j < cnt; j += 4) {
         int x = GetIntersection (j, row);
         if (int.MaxValue != x) intersections.Add (x);
      }
      if (intersections.Count > 0)
         mBmp.DrawHorizontalLines (row, mColor, intersections.OrderBy (a => a).ToArray ());
   }
   GrayBMP mBmp;
   int mColor;

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

