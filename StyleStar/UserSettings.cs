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

        public UserSettings() { }

        public string GetStepLeftString()
        {
            return "StepLeft" + Enum.GetNames(typeof(NoteColor))[(int)LeftColor];
        }

        public string GetStepRightString()
        {
            return "StepRight" + Enum.GetNames(typeof(NoteColor))[(int)RightColor];
        }
    }
}
