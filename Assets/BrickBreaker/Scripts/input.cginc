uniform RWStructuredBuffer<float> _InputAndTime;

//see GPUBrick.FillInputAndTime()

float GetDT()
{
    return _InputAndTime[0];
}

float GetLeftDown()
{
    return _InputAndTime[2];
}

float GetRightDown()
{
    return _InputAndTime[3];
}


float GetQDown()
{
    return _InputAndTime[4];
}

float GetDDown()
{
    return _InputAndTime[5];
}