namespace PlayAR.Common
{
    public class Timer
    {
        public bool startTimer = true;
        public float nowTimer = 0.0f, max_Timer = 0.0f;

        public void Set(bool start, float now, float end)
        {
            startTimer = start;
            nowTimer = now;
            max_Timer = end;
        }
    }
}