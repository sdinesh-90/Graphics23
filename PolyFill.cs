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
   public void AddLine (int x0, int y0, int x1, int y1) {
      if (y0 == y1) return;
      if (y0 > y1) (x0, y0, x1, y1) = (x1, y1, x0, y0);
      double m = (double)(y1 - y0) / (x1 - x0);
      mLinesData.Add ((1 / m, y0 - m * x0));
      mPoints.AddRange (new[] { x0, y0, x1, y1 });
   }

   /// <summary>After adding all the lines for polygon, fill it</summary>
   public void Fill (GrayBMP bmp, int color) {
      (mBmp, mColor) = (bmp, color);
      bmp.Begin ();
      // Fill the polygon
      List<int> positions = new ();
      for (int i = 0; i < bmp.Height; i++) {
         positions.Clear ();
         int cnt = mPoints.Count;
         for (int j = 0; j < cnt; j += 4) {
            int x = GetIntersection (j, i);
            if (int.MaxValue != x) positions.Add (x);
         }
         positions = positions.Order ().ToList ();
         for (int k = 0; k < positions.Count - 1; k += 2)
            mBmp.DrawHorizontalLine (positions[k], positions[k + 1], i, color);
      }
      bmp.End ();
   }
   #endregion

   #region Implementation -------------------------------------------
   void Fill (int row) {
      int cnt = mPoints.Count;
      List<int> positions = new ();
      for (int j = 0; j < cnt; j += 4) {
         int x = GetIntersection (j, row);
         if (int.MaxValue != x) positions.Add (x);
      }
      positions = positions.Order ().ToList ();
      for (int k = 0; k < positions.Count - 1; k += 2)
         mBmp.DrawHorizontalLine (positions[k], positions[k + 1], row, mColor);
   }
   GrayBMP mBmp;
   int mColor;

   // Get the intersection of line represented by idx (take four point in X, Y order)
   // with horizontal line
   int GetIntersection (int idx, int scanY) {
      int x1 = mPoints[idx], y1 = mPoints[idx + 1], x2 = mPoints[idx + 2], y2 = mPoints[idx + 3];
      // Check if scanY is in the range
      double dScanY = scanY + 0.5;
      if (dScanY < y1 || dScanY > y2) return int.MaxValue;
      int dx = x2 - x1;
      // Vartical line
      if (dx == 0) return x2;
      // Line Equation y = mx + c
      var (InvSlope, c) = mLinesData[idx / 4];
      return (int)((dScanY - c) * InvSlope);
   }
   #endregion

   #region Private Data ---------------------------------------------
   readonly List<int> mPoints = new ();
   readonly List<(double InvSlope, double c)> mLinesData = new ();
   #endregion
}
#endregion