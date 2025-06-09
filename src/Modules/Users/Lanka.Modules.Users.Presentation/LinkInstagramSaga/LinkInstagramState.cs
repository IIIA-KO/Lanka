using MassTransit;

namespace Lanka.Modules.Users.Presentation.LinkInstagramSaga;

public sealed class LinkInstagramState : SagaStateMachineInstance, ISagaVersion
{
    public Guid CorrelationId { get; set; }
    
    public int Version { get; set; }
    
    public string CurrentState { get; set; }
    
    public int LinkingCompletedStatus { get; set; }
}
