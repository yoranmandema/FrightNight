using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAttribute : MonoBehaviour {

    protected float _value;
    public float Value {
        get {
            return _value;
        }
        set {
            if (_value != value)
            {
                OnChanged();
            }

            if (value <= Min) {
                _value = Min;
                OnValueDepleted();
            } else if (value >= Max) {
                _value = Max;
                OnValueMaxed();
            } else {
                _value = value;
            }
        }
    }

    public float Default = 1;
    public float Max = 1;
    public float Min = 0;

	void Start () {
        Reset();
        Initialise();
    }

    public virtual void Initialise() {}
    public virtual void OnValueDepleted () {}
    public virtual void OnValueMaxed() {}
    public virtual void OnChanged() {}

    public void Reset() {
        Value = Default;
    }
}
