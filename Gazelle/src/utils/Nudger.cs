// Generated by .NET Reflector from D:\Sfered\Gazelle\Remnants\SferedApi_brep_manipulation.dll
namespace SferedApi
{
    using Rhino.Geometry;
    using System;
    
    internal class Nudger
    {
        public double StepDistance;
        public Vector3d XDir;
        public Vector3d YDir;
        public Point3d Origin;
        
        public Nudger(double aStepDistance, Plane aPlane)
        {
            this.StepDistance = aStepDistance;
            this.XDir = aPlane.get_XAxis();
            this.YDir = aPlane.get_YAxis();
            this.Origin = aPlane.get_Origin();
        }
        
        public Vector3d GetStep(int FinalStep)
        {
            int num = 0;
            Side right = Side.Right;
            int num2 = 0;
            int num3 = 1;
            int num4 = 0;
            int num5 = 0;
            int num6 = 0;
            while (true)
            {
                if (num6 >= FinalStep)
                {
                    Point3d pointd = new Point3d(this.Origin);
                    return (((this.XDir * num4) * this.StepDistance) + ((this.YDir * num5) * this.StepDistance));
                }
                if (right == Side.Top)
                {
                    num4++;
                }
                if (right == Side.Left)
                {
                    num5 += -1;
                }
                if (right == Side.Bottom)
                {
                    num4 += -1;
                }
                if (right == Side.Right)
                {
                    num5++;
                }
                num3++;
                if (num3 >= num2)
                {
                    if (right == Side.Top)
                    {
                        num3 = 0;
                        right = Side.Left;
                    }
                    else if (right == Side.Left)
                    {
                        num3 = 0;
                        right = Side.Bottom;
                    }
                    else if (right == Side.Bottom)
                    {
                        num3 = 0;
                        right = Side.Right;
                    }
                    else if ((right == Side.Right) && (num3 != num2))
                    {
                        num2 = (num + 8) / 4;
                        num3 = 1;
                        right = Side.Top;
                    }
                }
                num6++;
            }
        }
        
        private enum Side
        {
            Top,
            Right,
            Bottom,
            Left
        }
    }
}
