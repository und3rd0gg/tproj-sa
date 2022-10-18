using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace Obi
{
	/**
     * Implementation of asynchronous jobs that can return data, throw exceptions, and have a duration threshold 
     * below which they are run synchronously.
     */
	public class CoroutineJob
    {
        public int
            asyncThreshold =
                0; //Time in milliseconds that must pass before job switches to async mode. By default, the job is asynchronous from the start.

        private Exception e;

        private object result;
        private bool stop;

        public object Result
        {
            get
            {
                if (e != null) throw e;
                return result;
            }
        }

        public bool IsDone { get; private set; }

        public bool RaisedException { get; private set; }

        private void Init()
        {
            IsDone = false;
            RaisedException = false;
            stop = false;
            result = null;
        }

        /**
         * Runs the provided coroutine in a completely syncrhonous way, just like it would if it wasn't a coroutine, and 
         * returns a list of all coroutine results, in the order they were yielded. Will immediately rethrow any exceptions thrown by the coroutine.
         */
        public static object RunSynchronously(IEnumerator coroutine)
        {
            var results = new List<object>();

            if (coroutine == null) return results;

            try
            {
                while (coroutine.MoveNext()) results.Add(coroutine.Current);
            }
            catch (Exception e)
            {
                throw e;
            }

            return results;
        }

        public IEnumerator Start(IEnumerator coroutine)
        {
            Init();

            if (coroutine == null)
            {
                IsDone = true;
                yield break;
            }

            var sw = new Stopwatch();
            sw.Start();

            while (!stop)
            {
                try
                {
                    if (!coroutine.MoveNext())
                    {
                        IsDone = true;
                        sw.Stop();
                        yield break;
                    }
                }
                catch (Exception e)
                {
                    this.e = e;
                    RaisedException = true;
                    Debug.LogException(e);
                    IsDone = true;
                    sw.Stop();
                    yield break;
                }

                result = coroutine.Current;

                //If too much time has passed sine job start, switch to async mode:
                if (sw.ElapsedMilliseconds > asyncThreshold) yield return result;
            }
        }

        public void Stop()
        {
            stop = true;
        }

        public class ProgressInfo
        {
            public float progress;
            public string userReadableInfo;

            public ProgressInfo(string userReadableInfo, float progress)
            {
                this.userReadableInfo = userReadableInfo;
                this.progress = progress;
            }
        }
    }
}