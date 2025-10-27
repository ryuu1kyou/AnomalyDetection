using AnomalyDetection.Samples;
using Xunit;

namespace AnomalyDetection.EntityFrameworkCore.Domains;

[Collection(AnomalyDetectionTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<AnomalyDetectionEntityFrameworkCoreTestModule>
{

}
