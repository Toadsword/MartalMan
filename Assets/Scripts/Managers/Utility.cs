using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utility
{

    public static float pixelPerMeter = 64.0f;

    public static float StartTimer(float deltaTime)
    {
        return Time.time + deltaTime;
    }

    // Return "true" if the timer has passed the time
    public static bool IsOver(float timerToCheck)
    {
        return timerToCheck <= Time.time;
    }

    public static float GetTimerRemainingTime(float timer)
    {
        return timer - Time.time;
    }

    public static float ResetTimer()
    {
        return Time.time;
    }

    public static void AdaptPositionToPixel(Transform transform)
    {
        Vector2 newPos = transform.position;
        newPos.x = Mathf.RoundToInt(newPos.x * pixelPerMeter) / pixelPerMeter;
        newPos.y = Mathf.RoundToInt(newPos.y * pixelPerMeter) / pixelPerMeter;

        transform.position = newPos;
    }

    public static Vector2 AdaptPositionToPixel(Vector2 position)
    {
        position.x = Mathf.RoundToInt(position.x * pixelPerMeter) / pixelPerMeter;
        position.y = Mathf.RoundToInt(position.y * pixelPerMeter) / pixelPerMeter;

        return position;
    }

    public static float AdaptPositionToPixel(float position)
    {
        position = Mathf.RoundToInt(position * pixelPerMeter) / pixelPerMeter;
        return position;
    }
}
