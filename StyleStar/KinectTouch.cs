using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;

namespace StyleStar
{
    // This class translates values out of the Kinect into a usable input for StyleStar

    public class KinectTouch
    {
        public List<KinectTouchPoint> Points = new List<KinectTouchPoint>();
        public bool IsCalibrated { get; private set; } = false;
        public DepthImagePoint LastLeftAnkleRaw { get { if (kinect.BodyFrames.Count > 0) return kinect.BodyFrames.Last().LeftAnklePoint; else return new DepthImagePoint(); } }
        public float LeftAnkleRatio { get { return FeetCal.FindXRatio(LastLeftAnkleRaw); } }
        public DepthImagePoint LastRightAnkleRaw { get { if (kinect.BodyFrames.Count > 0) return kinect.BodyFrames.Last().RightAnklePoint; else return new DepthImagePoint(); } }
        public float RightAnkleRatio { get { return FeetCal.FindXRatio(LastRightAnkleRaw); } }
        public DepthImagePoint LastHipRaw { get { if (kinect.BodyFrames.Count > 0) return kinect.BodyFrames.Last().HipPoint; else return new DepthImagePoint(); } }

        public Skeleton LastSkeleton { get { if (kinect.Skeletons.Count > 0) return kinect.Skeletons.Last(); else return null; } }
        public float FloorZ { get { return kinect.FloorZ; } }

        private Kinect kinect;

        // Calibration points
        private CalibrationPoints FeetCal;
        private CalibrationPoints HipCal;

        public KinectTouch()
        {
            kinect = new Kinect();
            kinect.DataUpdate += UpdateFromKinect;
            FeetCal = new CalibrationPoints();
            HipCal = new CalibrationPoints();
        }

        public bool ReadCalibrationFile(string filename)
        {
            if (!File.Exists(filename))
                return false;

            int lineCount = 0;
            string line;
            using (StreamReader sr = new StreamReader(new FileStream(filename, FileMode.Open)))
            {
                while(sr.Peek() > -1)
                {
                    line = sr.ReadLine();
                    lineCount++;

                    switch (lineCount)
                    {
                        case 1:
                            FeetCal.FrontLeft = ParseLine(line);
                            break;
                        case 2:
                            FeetCal.FrontRight = ParseLine(line);
                            break;
                        case 3:
                            FeetCal.BackLeft = ParseLine(line);
                            break;
                        case 4:
                            FeetCal.BackRight = ParseLine(line);
                            break;
                        case 5:
                            HipCal.FrontLeft = ParseLine(line);
                            break;
                        case 6:
                            HipCal.FrontRight = ParseLine(line);
                            break;
                        case 7:
                            HipCal.BackLeft = ParseLine(line);
                            break;
                        case 8:
                            HipCal.BackRight = ParseLine(line);
                            break;
                        default:
                            break;
                    }
                }
            }

            if (lineCount < 8)
                return false;

            FeetCal.CalculateVals();
            HipCal.CalculateVals();
            IsCalibrated = true;
            return true;
        }

        public void WriteCalibrationFile(string filename)
        {
            // This overwrites the existing file
            using (StreamWriter sw = new StreamWriter(new FileStream(filename, FileMode.Create)))
            {
                sw.Write(FeetCal.ToString());
                sw.Write(HipCal.ToString());
            }
        }

        private DepthImagePoint ParseLine(string line)
        {
            string[] split = line.Split(',');
            if (split.Length != 3)
                return new DepthImagePoint();
            return new DepthImagePoint() { X = Convert.ToInt32(split[0]), Y = Convert.ToInt32(split[1]), Depth = Convert.ToInt32(split[2]) };
        }

        public bool SetCal(CalibrationStage stage)
        {
            if(stage == CalibrationStage.Start)
            {
                IsCalibrated = false;
                Points.Clear();
                return true;
            }

            if (kinect.BodyFrames.Count == 0)
                return false;

            switch (stage)
            {
                case CalibrationStage.Start:
                    break;
                case CalibrationStage.FrontLeft:
                    FeetCal.FrontLeft = kinect.BodyFrames.Last().LeftAnklePoint;
                    HipCal.FrontLeft = kinect.BodyFrames.Last().HipPoint;
                    break;
                case CalibrationStage.FrontRight:
                    FeetCal.FrontRight = kinect.BodyFrames.Last().RightAnklePoint;
                    HipCal.FrontRight = kinect.BodyFrames.Last().HipPoint;
                    break;
                case CalibrationStage.BackLeft:
                    FeetCal.BackLeft = kinect.BodyFrames.Last().LeftAnklePoint;
                    HipCal.BackLeft = kinect.BodyFrames.Last().HipPoint;
                    break;
                case CalibrationStage.BackRight:
                    FeetCal.BackRight = kinect.BodyFrames.Last().RightAnklePoint;
                    HipCal.BackRight = kinect.BodyFrames.Last().HipPoint;
                    IsCalibrated = true;
                    break;
                case CalibrationStage.Finish:
                    break;
                default:
                    break;
            }
            return true;
        }

        public string GetCalReadout()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Feet Cal");
            sb.AppendFormat("Front Left    X:  {0}   Y:  {1}   Depth:  {2}\n", FeetCal.FrontLeft.X, FeetCal.FrontLeft.Y, FeetCal.FrontLeft.Depth);
            sb.AppendFormat("Front Right   X:  {0}   Y:  {1}   Depth:  {2}\n", FeetCal.FrontRight.X, FeetCal.FrontRight.Y, FeetCal.FrontRight.Depth);
            sb.AppendFormat("Back Left     X:  {0}   Y:  {1}   Depth:  {2}\n", FeetCal.BackLeft.X, FeetCal.BackLeft.Y, FeetCal.BackLeft.Depth);
            sb.AppendFormat("Back Right    X:  {0}   Y:  {1}   Depth:  {2}\n", FeetCal.BackRight.X, FeetCal.BackRight.Y, FeetCal.BackRight.Depth);
            return sb.ToString();
        }

        private void UpdateFromKinect(object sender, EventArgs e)
        {
            // Don't do anything with data until we're calbirated
            if(IsCalibrated)
            {
                while (Points.Count < 2)
                    Points.Add(new KinectTouchPoint() { Cal = FeetCal }); // There's always 2, one for each foot

                Points[0].Update(kinect.BodyFrames.Last(), Side.Left);
                Points[1].Update(kinect.BodyFrames.Last(), Side.Right);
            }
        }
    }


    /// <summary>
    /// KinectTouchPoint takes the current point received and compares it to the calibration to generate an in-game coordinate
    /// </summary>
    public class KinectTouchPoint
    {
        /// <summary>
        /// X is a range from 0 to 1023, where 0 is full left and 1023 is full right (facing the screen)
        /// </summary>
        public float X { get; set; }
        public float KneeAngle { get; set; }

        public VerticalState VState { get; set; }
        public CalibrationPoints Cal { get; set; }
        public double StepTime { get; set; }

        public KinectTouchPoint() { }

        private float footUpThreshold = 10f;

        public void Update(BodyFrame newFrame, Side side)
        {
            // Calculating X is easy, it's just a ratio (I'm going to ignore the deviation caused by raising feet until I test)
            // I'm assuming it's a perfect trapezoid for simplicity, also
            var xRatio = Cal.FindXRatio(side == Side.Left ? newFrame.LeftAnklePoint : newFrame.RightAnklePoint);
            if(0 <= xRatio && xRatio <= 1.0)
                X = xRatio * 1023.0f;

            // Calculate Y of theortical floor
            var yFloor = Cal.FindFloorYAtDepth(side == Side.Left ? newFrame.LeftAnklePoint : newFrame.RightAnklePoint);

            if (((side == Side.Left ? newFrame.LeftAnklePoint : newFrame.RightAnklePoint).Y - yFloor) <= -footUpThreshold)
                VState = VerticalState.InAir;
            else
                VState = VerticalState.OnFloor;

            // Calculate angle of knee
            Vector3 originToAnkle, originToKnee, originToHip;
            Vector3 HipToKnee, KneeToAnkle;
            if (side == Side.Left)
            {
                //originToAnkle = new Vector3(newFrame.LeftAnklePoint.X, newFrame.LeftAnklePoint.Y, newFrame.LeftAnklePoint.Depth);
                //originToKnee = new Vector3(newFrame.LeftKneePoint.X, newFrame.LeftKneePoint.Y, newFrame.LeftKneePoint.Depth);
                //originToHip = new Vector3(newFrame.LeftHipPoint.X, newFrame.LeftHipPoint.Y, newFrame.LeftHipPoint.Depth);
                originToAnkle = new Vector3(newFrame.LeftAnklePoint.X, newFrame.LeftAnklePoint.Y, 0);
                originToKnee = new Vector3(newFrame.LeftKneePoint.X, newFrame.LeftKneePoint.Y, 0);
                originToHip = new Vector3(newFrame.LeftHipPoint.X, newFrame.LeftHipPoint.Y, 0);
                HipToKnee = originToHip - originToKnee;
                KneeToAnkle = originToKnee - originToAnkle;
                HipToKnee.Normalize();
                KneeToAnkle.Normalize();
                var dot = HipToKnee.Dot(KneeToAnkle);
                var angle = Math.Acos(dot / (HipToKnee.Length() * KneeToAnkle.Length()));
                KneeAngle = (float)(angle * 180.0 / Math.PI);
            }

            if (VState == VerticalState.OnFloor)
                StepTime = Globals.TotalGameMS;

        }
    }

    public class CalibrationPoints
    {
        // Right and left are in game-space terms (aka facing the screen)
        public DepthImagePoint FrontRight { get; set; }
        public DepthImagePoint FrontLeft { get; set; }
        public DepthImagePoint BackRight { get; set; }
        public DepthImagePoint BackLeft { get; set; }

        private int b1, b2, h;
        private float depthSlope;

        public void CalculateVals()
        {
            //b1 = BackLeft.X - BackRight.X;
            //b2 = FrontLeft.X - FrontRight.X;
            b1 = BackRight.X - BackLeft.X;
            b2 = FrontRight.X - FrontLeft.X;
            h = FrontLeft.Y - BackLeft.Y;
            depthSlope = (float)(BackLeft.Y - FrontLeft.Y) / (float)(BackLeft.Depth - FrontLeft.Depth);
        }

        public float FindWidthAtPoint(DepthImagePoint pt)
        {
            int y = FrontLeft.Y - pt.Y;

            return ((float)y / (float)h) * (float)b1 + ((float)h - (float)y) / (float)h * (float)b2;
        }

        public float FindFloorYAtDepth(DepthImagePoint pt)
        {
            return (float)(pt.Depth - FrontLeft.Depth) * depthSlope + (float)FrontLeft.Y;
        }

        public float FindXRatio(DepthImagePoint pt)
        {
            var width = FindWidthAtPoint(pt);
            //if (width > b2)
            //    return -1.0f;
            int m = BackLeft.X + b1 / 2;
            int x1 = pt.X - m;
            float x2 = x1 + (width / 2);

            return x2 / width;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(FrontLeft.X.ToString() + "," + FrontLeft.Y.ToString() + "," + FrontLeft.Depth.ToString());
            sb.AppendLine(FrontRight.X.ToString() + "," + FrontRight.Y.ToString() + "," +FrontRight.Depth.ToString());
            sb.AppendLine(BackLeft.X.ToString() + "," + BackLeft.Y.ToString() + "," + BackLeft.Depth.ToString());
            sb.AppendLine(BackRight.X.ToString() + "," + BackRight.Y.ToString() + "," + BackRight.Depth.ToString());

            return sb.ToString();
        }
    }

    public enum VerticalState
    {
        Unknown,
        OnFloor,
        InAir
    }

    public enum CalibrationStage
    {
        Start,
        FrontLeft,
        FrontRight,
        BackLeft,
        BackRight,
        Finish
    }
}
