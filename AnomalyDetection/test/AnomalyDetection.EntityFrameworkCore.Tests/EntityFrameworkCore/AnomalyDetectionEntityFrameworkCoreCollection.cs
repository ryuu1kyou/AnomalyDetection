using Xunit;

namespace AnomalyDetection.EntityFrameworkCore;

[CollectionDefinition(AnomalyDetectionTestConsts.CollectionDefinitionName)]
public class AnomalyDetectionEntityFrameworkCoreCollection : ICollectionFixture<AnomalyDetectionEntityFrameworkCoreFixture>
{

}
