using Lanka.Modules.Analytics.IntegrationEvents;
using Lanka.Modules.Users.IntegrationEvents.RenewInstagramAccess;
using MassTransit;

namespace Lanka.Modules.Users.Presentation.RenewInstagramAccessSaga;

public sealed class RenewInstagramAccessSaga : MassTransitStateMachine<RenewInstagramAccessState>
{
    public State RenewalStarted { get; init; }
    public State InstagramAccountDataFetched { get; init; }

    public Event<InstagramAccessRenewedIntegrationEvent> EventAccessRenewed { get; init; }
    public Event<InstagramAccountDataFetchedIntegrationEvent> EventAccountDataFetched { get; init; }
    public Event InstagramAccessRenewalCompleted { get; init; }

    public RenewInstagramAccessSaga()
    {
        this.Event(() => this.EventAccessRenewed, c => c.CorrelateById(m => m.Message.UserId));
        this.Event(() => this.EventAccountDataFetched, c => c.CorrelateById(m => m.Message.UserId));

        this.InstanceState(s => s.CurrentState);

        this.Initially(
            this.When(this.EventAccessRenewed)
                .Publish(context =>
                    new InstagramAccessRenewalStartedIntegrationEvent(
                        context.Message.Id,
                        context.Message.OccurredOnUtc,
                        context.Message.UserId,
                        context.Message.Code
                    )
                )
                .TransitionTo(this.RenewalStarted)
        );
        
        this.During(this.RenewalStarted,
            this.When(this.EventAccountDataFetched)
                .TransitionTo(this.InstagramAccountDataFetched)
        );

        this.CompositeEvent(
            () => this.InstagramAccessRenewalCompleted,
            state => state.RenewalCompletedStatus,
            this.EventAccountDataFetched
        );

        this.DuringAny(
            this.When(this.InstagramAccessRenewalCompleted)
                .Publish(context =>
                    new InstagramAccessRenewalCompletedIntegrationEvent(
                        Guid.NewGuid(),
                        DateTime.UtcNow,
                        context.Saga.CorrelationId
                    )
                )
                .Finalize()
        );
    }
}
