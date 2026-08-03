using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace FateDeck.Runtime.Views
{
    /// <summary>
    /// A tiny tween pump for UI Toolkit feedback: fades, slides, pops, pulses, shakes and
    /// floating damage numbers. The table view calls <see cref="Update"/> once per frame;
    /// everything else is fire-and-forget.
    /// </summary>
    public static class UiFx
    {
        private enum TweenKind
        {
            FadeIn,
            SlideIn,
            Pop,
            Pulse,
            FloatAway,
            Shake
        }

        private sealed class Tween
        {
            public VisualElement Target;
            public TweenKind Kind;
            public float Elapsed;
            public float Duration;
            public float A;
            public float B;
            public bool RemoveTargetOnDone;
        }

        private static readonly List<Tween> Tweens = new List<Tween>();

        public static void Update(float deltaTime)
        {
            for (int i = Tweens.Count - 1; i >= 0; i--)
            {
                Tween tween = Tweens[i];
                if (tween.Target == null)
                {
                    Tweens.RemoveAt(i);
                    continue;
                }

                tween.Elapsed += deltaTime;
                float t = tween.Duration > 0f ? Mathf.Clamp01(tween.Elapsed / tween.Duration) : 1f;
                Apply(tween, t);

                if (t >= 1f)
                {
                    if (tween.RemoveTargetOnDone)
                    {
                        tween.Target.RemoveFromHierarchy();
                    }

                    Tweens.RemoveAt(i);
                }
            }
        }

        private static void Apply(Tween tween, float t)
        {
            VisualElement target = tween.Target;
            switch (tween.Kind)
            {
                case TweenKind.FadeIn:
                    target.style.opacity = Mathf.Lerp(tween.A, tween.B, EaseOut(t));
                    break;

                case TweenKind.SlideIn:
                    target.style.translate = new Translate(0f, Mathf.Lerp(tween.A, 0f, EaseOut(t)));
                    break;

                case TweenKind.Pop:
                {
                    float scale = Mathf.LerpUnclamped(tween.A, 1f, Overshoot(t));
                    target.style.scale = new Scale(new Vector2(scale, scale));
                    break;
                }

                case TweenKind.Pulse:
                {
                    float wave = Mathf.Sin(t * Mathf.PI);
                    float scale = 1f + (tween.A - 1f) * wave;
                    target.style.scale = new Scale(new Vector2(scale, scale));
                    break;
                }

                case TweenKind.FloatAway:
                    target.style.translate = new Translate(tween.B, tween.A * EaseOut(t));
                    target.style.opacity = 1f - t * t;
                    break;

                case TweenKind.Shake:
                {
                    float falloff = 1f - t;
                    float x = Mathf.Sin(t * 40f) * tween.A * falloff;
                    target.style.translate = new Translate(x, 0f);
                    break;
                }
            }
        }

        private static float EaseOut(float t)
        {
            float inverse = 1f - t;
            return 1f - inverse * inverse * inverse;
        }

        private static float Overshoot(float t)
        {
            const float back = 1.35f;
            float shifted = t - 1f;
            return 1f + shifted * shifted * ((back + 1f) * shifted + back);
        }

        private static void Add(VisualElement target, TweenKind kind, float duration, float a, float b = 0f,
            bool removeOnDone = false)
        {
            if (target == null)
            {
                return;
            }

            Tweens.Add(new Tween
            {
                Target = target,
                Kind = kind,
                Duration = Mathf.Max(0.01f, duration),
                A = a,
                B = b,
                RemoveTargetOnDone = removeOnDone
            });
        }

        /// <summary>Fades an element in while sliding it down from a small offset.</summary>
        public static void FadeSlideIn(VisualElement element, float fromY = -14f, float duration = 0.28f)
        {
            if (element == null)
            {
                return;
            }

            element.style.opacity = 0f;
            element.style.translate = new Translate(0f, fromY);
            Add(element, TweenKind.FadeIn, duration, 0f, 1f);
            Add(element, TweenKind.SlideIn, duration, fromY);
        }

        /// <summary>Scales an element in from small with a slight overshoot.</summary>
        public static void Pop(VisualElement element, float fromScale = 0.72f, float duration = 0.26f)
        {
            if (element == null)
            {
                return;
            }

            element.style.scale = new Scale(new Vector2(fromScale, fromScale));
            Add(element, TweenKind.Pop, duration, fromScale);
        }

        /// <summary>One quick heartbeat: scale up and back.</summary>
        public static void Pulse(VisualElement element, float upTo = 1.12f, float duration = 0.3f)
        {
            Add(element, TweenKind.Pulse, duration, upTo);
        }

        /// <summary>Floats an element up (dy negative) while fading, then removes it.</summary>
        public static void FloatAway(VisualElement element, float offsetX, float dy = -52f,
            float duration = 0.95f)
        {
            Add(element, TweenKind.FloatAway, duration, dy, offsetX, removeOnDone: true);
        }

        /// <summary>A decaying horizontal shake (getting hit).</summary>
        public static void Shake(VisualElement element, float magnitude = 7f, float duration = 0.4f)
        {
            Add(element, TweenKind.Shake, duration, magnitude);
        }

        public static void Clear()
        {
            Tweens.Clear();
        }
    }
}
