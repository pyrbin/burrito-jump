float clamp01(float In)
{
    return clamp(In, 0.0, 1.0);
}

float posterize(float In, float Steps)
{
    return floor(In / (1 / Steps)) * (1 / Steps);
}
