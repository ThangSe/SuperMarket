using System;

[Serializable]
public class FloatReference
{
    public bool UseConstant;
    public float ConstantValue;
    public SOFloatVariable Variable;
    public float Value
    {
        get { return UseConstant ? ConstantValue : Variable.Value; }
    }
}
