using AnomalyDetection.Samples;
using Xunit;

namespace AnomalyDetection.EntityFrameworkCore.Applications;

[Collection(AnomalyDetectionTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<AnomalyDetectionEntityFrameworkCoreTestModule>
{

}
