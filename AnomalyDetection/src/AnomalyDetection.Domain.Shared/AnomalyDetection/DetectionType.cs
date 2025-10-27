namespace AnomalyDetection.AnomalyDetection;

public enum DetectionType
{
    OutOfRange = 1,
    RateOfChange = 2,
    Timeout = 3,
    Stuck = 4,
    Periodic = 5,
    Custom = 99
}