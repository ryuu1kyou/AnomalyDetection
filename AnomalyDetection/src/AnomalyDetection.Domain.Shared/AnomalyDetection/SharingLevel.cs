namespace AnomalyDetection.AnomalyDetection;

public enum SharingLevel
{
    Private = 0,        // テナント内のみ
    OemPartner = 1,     // OEMパートナー間
    Industry = 2,       // 業界共通
    Public = 3          // パブリック
}