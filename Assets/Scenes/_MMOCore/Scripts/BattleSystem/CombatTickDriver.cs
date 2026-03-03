using System;
using System.Collections;
using UnityEngine;

public struct CombatTick
{
    public readonly int Index;       // 0,1,2...
    public readonly float TimeSec;   // tick 시작 시각(클라 기준)
    public readonly float DtSec;     // tick 간격(고정)

    public CombatTick(int index, float timeSec, float dtSec)
    {
        Index = index;
        TimeSec = timeSec;
        DtSec = dtSec;
    }
}

public class CombatTickDriver : MonoBehaviour
{
    Coroutine _co;

    public int TickIndex { get; private set; }
    public float TickInterval { get; private set; }

    public void Begin(float intervalSec, Action<CombatTick> onTick)
    {
        Stop();
        TickInterval = Mathf.Max(0.01f, intervalSec);
        TickIndex = 0;
        _co = StartCoroutine(CoTick(onTick));
    }

    public void Stop()
    {
        if (_co != null) StopCoroutine(_co);
        _co = null;
    }

    IEnumerator CoTick(Action<CombatTick> onTick)
    {
        float next = Time.time;
        while (true)
        {
            float now = Time.time;

            // 너무 늦었으면(프레임 드랍) 따라잡되, 폭주 방지
            int safety = 0;
            while (now >= next && safety++ < 5)
            {
                onTick?.Invoke(new CombatTick(TickIndex++, next, TickInterval));
                next += TickInterval;
            }

            // 다음 tick까지 대기
            float wait = Mathf.Max(0f, next - Time.time);
            yield return (wait > 0f) ? new WaitForSeconds(wait) : null;
        }
    }
}