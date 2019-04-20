using Microsoft.Kinect;
using System;
using System.IO;

namespace StyleStar
{
    // This class handles actual connection to the Kinect and generating a list of raw (and possibly processed?) data

    public class Kinect
    {
        public bool Connected
        {
            get
            {
                if (sensor == null) return false; else return sensor.IsRunning;
            }
        }

        public FixedSizedQueue<BodyFrame> BodyFrames { get; private set; }
        public FixedSizedQueue<Skeleton> Skeletons { get; private set; }
        public float FloorZ { get; private set; }


        public EventHandler<EventArgs> DataUpdate;

        private KinectSensor sensor;

        public Kinect()
        {
            BodyFrames = new FixedSizedQueue<BodyFrame>(20);
            Skeletons = new FixedSizedQueue<Skeleton>(20);

            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor)
            {
                // Turn on the skeleton stream to receive skeleton frames
                this.sensor.SkeletonStream.Enable();

                // Add an event handler to be called whenever there is new color frame data
                this.sensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;

                // Start the sensor!
                try
                {
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }
        }

        /// <summary>
        /// Event handler for Kinect sensor's SkeletonFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skeletons = new Skeleton[0];

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);

                    FloorZ = skeletonFrame.FloorClipPlane.Item3;
                }
            }

            if (skeletons.Length != 0)
            {
                foreach (var skel in skeletons)
                {
                    if (skel.TrackingState == SkeletonTrackingState.Tracked)
                    {
                        Skeletons.Enqueue(skel);

                        BodyFrame tempFrame = new BodyFrame();
                        tempFrame.LeftAnklePoint = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skel.Joints[JointType.AnkleLeft].Position, DepthImageFormat.Resolution640x480Fps30);
                        tempFrame.LeftKneePoint = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skel.Joints[JointType.KneeLeft].Position, DepthImageFormat.Resolution640x480Fps30);
                        tempFrame.LeftHipPoint = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skel.Joints[JointType.HipLeft].Position, DepthImageFormat.Resolution640x480Fps30);
                        tempFrame.RightAnklePoint = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skel.Joints[JointType.AnkleRight].Position, DepthImageFormat.Resolution640x480Fps30);
                        tempFrame.RightKneePoint = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skel.Joints[JointType.KneeRight].Position, DepthImageFormat.Resolution640x480Fps30);
                        tempFrame.RightHipPoint = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skel.Joints[JointType.HipRight].Position, DepthImageFormat.Resolution640x480Fps30);
                        tempFrame.HipPoint = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skel.Joints[JointType.HipCenter].Position, DepthImageFormat.Resolution640x480Fps30);

                        BodyFrames.Enqueue(tempFrame);

                        DataUpdate?.Invoke(this, new EventArgs());
                    }
                }
            }
        }
    }

    public class BodyFrame
    {
        public DepthImagePoint LeftAnklePoint { get; set; }
        public DepthImagePoint LeftKneePoint { get; set; }
        public DepthImagePoint LeftHipPoint { get; set; }
        public DepthImagePoint RightAnklePoint { get; set; }
        public DepthImagePoint RightKneePoint { get; set; }
        public DepthImagePoint RightHipPoint { get; set; }
        public DepthImagePoint HipPoint { get; set; }
    }
}
