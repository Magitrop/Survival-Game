using Game.Miscellaneous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Interfaces
{
    public class Animation
    {
        public List<(ImageFrame frame, float frameDuration)> frames { get; set; }
        public string animationName;

        public Animation(string name)
        {
            animationName = name;
        }
    }

    public interface IAnimatable
    {
        List<Animation> animations { get; set; }
        // должен вызываться в Update
        void PlayAnimation();
        void InitializeAnimation();
    }
}