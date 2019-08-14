using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StyleStar
{
    public enum NoteColor { Pink, Blue, Yellow, Green, Orange, Purple, Black, White};

    public class UserSettings
    {
        public NoteColor LeftColor = NoteColor.Pink;
        public NoteColor RightColor = NoteColor.Blue;

        public string LeftColorString { get { return Enum.GetNames(typeof(NoteColor))[(int)LeftColor]; } }
        public string RightColorString { get { return Enum.GetNames(typeof(NoteColor))[(int)RightColor]; } }

        public UserSettings() { }

        public string GetStepLeftString()
        {
            return "StepLeft" + LeftColorString;
        }

        public string GetStepRightString()
        {
            return "StepRight" + RightColorString;
        }

        public string GetHoldLeftString()
        {
            return "Hold" + LeftColorString;
        }

        public string GetHoldRightString()
        {
            return "Hold" + RightColorString;
        }

        public string GetSlideLeftString()
        {
            return "Hold" + LeftColorString;
        }

        public string GetSlideRightString()
        {
            return "Hold" + RightColorString;
        }
    }
}
