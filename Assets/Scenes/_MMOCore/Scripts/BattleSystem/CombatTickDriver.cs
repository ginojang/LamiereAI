using System;
using System.Collections;
using UnityEngine;

public class CombatTickDriver : MonoBehaviour
{
    private Coroutine _co;

    public void Begin(float interval, Action<float> onTick)
    {
        Stop();
        _co = StartCoroutine(CoTick(interval, onTick));
    }

    public void Stop()
    {
        if (_co != null) StopCoroutine(_co);
        _co = null;
    }

    private IEnumerator CoTick(float interval, Action<float> onTick)
    {
        var wait = new WaitForSeconds(interval);
        while (true)
        {
            onTick?.Invoke(Time.time);
            yield return wait;
        }
    }
}