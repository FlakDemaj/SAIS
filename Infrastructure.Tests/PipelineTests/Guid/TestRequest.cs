using Application.Common.Interfaces;

namespace Infrastructure.Tests.PipelineTests.Guid;

public class TestRequest : IHasGuid
{
    public int PublicId { get; set; }
    public System.Guid Guid { get; set; }
    public string EntityType { get; set; } = string.Empty;
}
