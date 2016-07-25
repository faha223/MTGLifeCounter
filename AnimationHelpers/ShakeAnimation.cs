using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Views.Animations;
using Android.Animation;

namespace MTGLifeCounter.AnimationHelpers
{
    public class ShakeAnimation : TranslateAnimation
    {
        class ShakeInterpolator : Java.Lang.Object, IInterpolator
        {
            double s = 9.5;

            public ShakeInterpolator(int shakes)
            {
                if (shakes <= 0)
                    throw new ArgumentException("shakes must be at least 1");
                s = shakes + 0.5;
            }

            public float GetInterpolation(float input)
            {
                return (float)Math.Cos(9.5 * Math.PI * input) * (1.0f - input);
            }
        }

        public ShakeAnimation(float displacement, int shakes) : base(0, displacement, 0, 0)
        {
            Interpolator = new ShakeInterpolator(shakes);
        }
    }
}