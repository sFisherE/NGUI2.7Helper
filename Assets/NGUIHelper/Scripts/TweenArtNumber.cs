using System;
using System.Collections.Generic;
using UnityEngine;
using Prime31.GoKitLite;


class TweenArtNumber : GokitCtrlBase
{
    public float from;
    public float to;
    float mDist;
    //public UILabel label;
    public UIArtText artText;
    public int fractionLength = 0;//小数位数
    //public string format = "{0}"; //"prefix{0}postfix"

    int mValue;
    public int Value
    {
        set
        {
            mValue = value;
            //label.text = string.Format(format, value);
            artText.text = value.ToString();
        }
        get { return mValue; }
    }

    public float currentValue;

    float mTempDuration;
    float mSoundInterval;
    protected override void PlaySound()
    {
        if ((audioClip != null) && (mSoundInterval > 0))
        {
            InvokeRepeating("_PlaySound", delay, mSoundInterval);
            Invoke("StopSound", delay + mTempDuration);
        }
    }
    void StopSound()
    {
        CancelInvoke("_PlaySound");
    }

    void OnDisable()
    {
        CancelInvoke("_PlaySound");
        CancelInvoke("StopSound");
    }

    protected override void Begin()
    {
        mSoundInterval = 0.05f;

        mDist = Mathf.Abs(from - to);
        mTempDuration = mDist * 0.05f;
        if (mTempDuration > duration)
        {
            mTempDuration = duration;
        }
        else if (mTempDuration < 0.1f)
        {
            mTempDuration = 0.1f;
            mSoundInterval = 0.1f;
        }

        if (mDist == 0)
            mSoundInterval = -1;

        System.Action<Transform, float> action = (trans, dt) =>
        {
            if (!forward)
            {
                dt = 1 - dt;
            }
            float value = (to - from) * animationCurve.Evaluate(dt) + from;
            currentValue = value;
            if (fractionLength < 0)
            {
                fractionLength = 0;
            }
            string num = value.ToString(string.Format("F{0}", fractionLength));
            artText.text = num;
        };
        mTween = GoKitLite.instance.customAction(transform, mTempDuration, action, delay, mEasingFunc)
            .setLoopType(loopType, loopNum); ;
        //mTween.setCompletionHandler(delegate(Transform tf) { mTween = null; });
    }

    public void Begin(float duration, float to)
    {
        this.duration = duration;
        this.to = to;
        this.Begin();
        this.PlaySound();
        SetCompleteHandler();
    }

}
