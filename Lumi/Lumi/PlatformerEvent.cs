using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Neat.Mathematics;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
namespace Lumi
{
    public class PlatformerEvent
    {
        public static PlatformerWorld World;
        public static PlatformerEngine Engine;
        public TimeSpan ActiveTime = TimeSpan.Zero;
        public TimeSpan Ttl = TimeSpan.Zero;

        public bool DisableOnTrigger = true;
        public bool CollectOnTrigger = true;
        public bool Enabled = true;
        bool _collect = false;
        public bool Collect
        {
            get { return _collect; }
            set { _collect = value; if (_collect && OnCollect != null) OnCollect(); }
        }

        public bool PerformTaskOnFirstTick = false;
        bool firstTick = true;
        public Action OnCollect = null;
        public PlatformerEvent()
        {

        }

        public virtual void Update(GameTime gameTime)
        {
            /*Debug.Assert(
                World != null &&
                Physics != null &&
                Engine != null, "Error initializing PlatformerEvent");*/

            if (!Enabled) return;

            if (firstTick && PerformTaskOnFirstTick) PerformTask(gameTime);
            firstTick = false;

            ActiveTime += gameTime.ElapsedGameTime;
            if (Ttl != TimeSpan.Zero && ActiveTime > Ttl)
            {
                Collect = true;
                Enabled = false;
            }

            if (CheckCondition(gameTime))
            {
                PerformTask(gameTime);
                if (DisableOnTrigger) Enabled = false;
                if (CollectOnTrigger) Collect = true;
            }
        }

        public virtual bool CheckCondition(GameTime gameTime)
        {
            return false;
        }

        public virtual void PerformTask(GameTime gameTime)
        {
        }
    }

    public class TransitionTimerEvent : PlatformerEvent
    {
        public TimeSpan StartTime;
        public TimeSpan EndTime;
        public TimeSpan Period;
        TimeSpan timer;

        public Action<TransitionTimerEvent, GameTime> Task;

        public TransitionTimerEvent(TimeSpan start, TimeSpan end, TimeSpan period, Action<TransitionTimerEvent, GameTime> task = null)
        {
            StartTime = start;
            EndTime = end;
            Task = task;
            Period = period;
            timer = new TimeSpan(0);

            DisableOnTrigger = false;
            CollectOnTrigger = false;
        }

        public TransitionTimerEvent(TimeSpan start, TimeSpan end, Action<TransitionTimerEvent, GameTime> task = null)
        {
            StartTime = start;
            EndTime = end;
            Task = task;
            Period = TimeSpan.Zero;

            DisableOnTrigger = false;
            CollectOnTrigger = false;
        }

        public override bool CheckCondition(GameTime gameTime)
        {
            if (ActiveTime > StartTime)
            {
                if (ActiveTime > EndTime)
                    Collect = true;
                if (Period == TimeSpan.Zero)
                {
                    return true;
                }
                else
                {
                    timer += gameTime.ElapsedGameTime;
                    if (timer > Period)
                    {
                        timer = TimeSpan.Zero;
                        return true;
                    }
                }
            }
            return false;
        }

        public override void PerformTask(GameTime gameTime)
        {
            if (Task != null) Task(this, gameTime);
            base.PerformTask(gameTime);
        }
    }

    public class TimerEvent : PlatformerEvent
    {
        public TimeSpan Period;
        public double PeriodMin = 0, PeriodMax = 0;
        public int Count { get; private set; }
        TimeSpan timer;
        public Action<TimerEvent, GameTime> Task;

        public TimerEvent(TimeSpan period, Action<TimerEvent, GameTime> task = null)
        {
            Task = task;
            this.Period = period;
            this.timer = new TimeSpan(0);

            CollectOnTrigger = false;
            DisableOnTrigger = false;

            Count = 0;
        }

        public TimerEvent(TimeSpan periodMin, TimeSpan periodMax, Action<TimerEvent, GameTime> task = null)
        {
            Task = task;
            this.PeriodMin = periodMin.TotalMilliseconds;
            this.PeriodMax = periodMax.TotalMilliseconds;
            chooseRandomPeriod();
            this.timer = new TimeSpan(0);
            CollectOnTrigger = false;
            DisableOnTrigger = false;

            Count = 0;
        }

        void chooseRandomPeriod()
        {
            var d = PeriodMax - PeriodMin;
            Period = TimeSpan.FromMilliseconds(PeriodMin + (d * Engine.RandomGenerator.NextDouble()));
        }

        public override bool CheckCondition(GameTime gameTime)
        {
            if (timer > Period)
            {
                Count++;
                timer = TimeSpan.Zero;
                if (PeriodMin != 0 || PeriodMax != 0)
                    chooseRandomPeriod();
                return true;
            }
            timer += gameTime.ElapsedGameTime;
            return false;
        }

        public override void PerformTask(GameTime gameTime)
        {
            if (Task != null) Task(this, gameTime);
            base.PerformTask(gameTime);
        }
    }

    public class TimerOneTimeEvent : TimerEvent
    {
        public TimerOneTimeEvent(TimeSpan time, Action<TimerEvent, GameTime> task = null)
            : base(time, task)
        {
            CollectOnTrigger = true;
            DisableOnTrigger = true;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            Debug.Assert(Count <= 1);
        }
    }
}
