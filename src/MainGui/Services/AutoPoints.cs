
using System.Windows;

namespace MainGui.Services;

public class AutoPoints
{
    /// <summary>
    ///     座標點的換算，實現自動吸附功能
    /// </summary>
    public Point GetAutoAdsorbPoint(Point oldPoint)
    {
        int newX = RoundToNearestInteger(oldPoint.X);
        int newY = RoundToNearestInteger(oldPoint.Y);

        return new Point(newX, newY);
    }

    private int RoundToNearestInteger(double value)
    {
        if (value * 10 % 10 >= 5)
        {
            return (int)value + 1;
        }
        else
        {
            return (int)value;
        }
    }

}