using MassTransit;

namespace Lanka.Modules.Users.Presentation.RenewInstagramAccessSaga;

public sealed class RenewInstagramAccessState : SagaStateMachineInstance, ISagaVersion
{
    public Guid CorrelationId { get; set; }

    public int Version { get; set; }

    public string CurrentState { get; set; }

    public int RenewalCompletedStatus { get; set; }
    
    public DateTime StartedAt { get; set; }
    
    public Guid? TimeoutTokenId { get; set; }
}
