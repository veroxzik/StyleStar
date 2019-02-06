using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StyleStar
{
    // Grade is the physical grade that displays and disappears
    public class Grade
    {
        public bool IsVisible { get; private set; }
        public Texture2D Texture { get; private set; }
        private readonly int timeoutMS = 500;   // Time to disappear
        public double StartTime { get; private set; }
        private Rectangle drawRectangle;

        public void Set(GameTime time, HitGrade grade, int laneIndex, int width)
        {
            StartTime = time.TotalGameTime.TotalMilliseconds;
            IsVisible = true;
            switch (grade)
            {
                case HitGrade.Bad:
                    Texture = Globals.Textures["BadGrade"];
                    break;
                case HitGrade.Good:
                    Texture = Globals.Textures["GoodGrade"];
                    break;
                case HitGrade.Great:
                    Texture = Globals.Textures["GreatGrade"];
                    break;
                case HitGrade.Perfect:
                    Texture = Globals.Textures["PerfectGrade"];
                    break;
                default:
                    break;
            }
            // Somehow convert Note into drawRectangle
            if (drawRectangle == null)
                drawRectangle = new Rectangle();

            // 250 is far left, 930 is far right
            float spacing = 45.3f; // 42.5? what should this be
            drawRectangle.X = (int)(250 + laneIndex * spacing + spacing * (width - 1) / 2); // TODO this needs to get fixed
            drawRectangle.Y = 500;
            drawRectangle.Width = 100;
            drawRectangle.Height = 20;
        }

        public void Draw(SpriteBatch sb, GameTime time)
        {
            if ((time.TotalGameTime.TotalMilliseconds - StartTime) > timeoutMS)
                IsVisible = false;
            else
                sb.Draw(Texture, drawRectangle, Color.White);
        }

    }

    public class GradeCollection
    {
        private const int MaxGrades = 16;   // Only 16 grades can be visible at one time
        private Grade[] Grades = new Grade[MaxGrades];

        public GradeCollection()
        {
            for (int i = 0; i < MaxGrades; i++)
            {
                Grades[i] = new Grade();
            }
        }

        public void Set(GameTime time, Note note)
        {
            HitGrade grade = HitGrade.Bad;
            if (note.Motion == Motion.NotSet)
            {
                if (Math.Abs(note.HitResult.Difference) <= NoteTiming.Perfect)
                    grade = HitGrade.Perfect;
                else if (Math.Abs(note.HitResult.Difference) <= NoteTiming.Great)
                    grade = HitGrade.Great;
                else if (Math.Abs(note.HitResult.Difference) <= NoteTiming.Good)
                    grade = HitGrade.Good;
            }
            else
                grade = HitGrade.Perfect; // Successful motion hits are always Perfects

            for (int i = 0; i < MaxGrades; i++)
            {
                if (!Grades[i].IsVisible)
                {
                    Grades[i].Set(time, grade, note.LaneIndex, note.Width);
                    break;
                }
            }
        }

        public void Draw(SpriteBatch sb, GameTime time)
        {
            var list = Grades.Where(x => x.IsVisible).OrderBy(x => x.StartTime);
            foreach (var grade in list)
            {
                grade.Draw(sb, time);
            }
        }
    }
}
