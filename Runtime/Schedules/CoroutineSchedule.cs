using System;
using System.Collections;
using UnityEngine;

namespace Lab5Games.Schedules
{
    public class CoroutineSchedule : Schedule
    {
        IEnumerator _routine;
        Coroutine _coroutine;

        public CoroutineSchedule(IEnumerator routine)
        {
            _routine = routine;
        }

        protected override void OnStart()
        {
            _coroutine = ScheduleSystem.system.StartCoroutine(Routine());
        }

        protected override void OnCancel()
        {
            if(_coroutine != null)
            {
                ScheduleSystem.system.StopCoroutine(_coroutine);
                _coroutine = null;
            }
        }

        IEnumerator Routine()
        {
            bool running = true;

            while(running)
            {
                if(State == States.Canceled)
                {
                    break;
                }
                

                if(Paused)
                {
                    yield return null;
                }
                else
                {
                    if(_routine.MoveNext())
                    {
                        yield return null;
                    }
                    else
                    {
                        running = false;
                        State = States.Completed;
                    }
                }
            }
        }
    }
}