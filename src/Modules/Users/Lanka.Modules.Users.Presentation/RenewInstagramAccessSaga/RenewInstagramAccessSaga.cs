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
    public Event<InstagramRenewalFailedIntegrationEvent> EventRenewalFailed { get; init; }
    public Event InstagramAccessRenewalCompleted { get; init; }

    public Schedule<RenewInstagramAccessState, InstagramRenewalTimeoutEvent> RenewalTimeout { get; init; }

    public RenewInstagramAccessSaga()
    {
        this.Event(() => this.EventAccessRenewed, c => c.CorrelateById(m => m.Message.UserId));
        this.Event(() => this.EventAccountDataFetched, c => c.CorrelateById(m => m.Message.UserId));
        this.Event(() => this.EventRenewalFailed, c => c.CorrelateById(m => m.Message.UserId));

        this.Schedule(
            () => this.RenewalTimeout,
            instance => instance.TimeoutTokenId,
            s =>
            {
                s.Delay = TimeSpan.FromMinutes(2);
                s.Received = r => r.CorrelateById(context => context.Message.CorrelationId);
            }
        );

        this.InstanceState(s => s.CurrentState);

        this.SetCompletedWhenFinalized();

        this.Initially(
            this.When(this.EventAccessRenewed)
                .Then(context => context.Saga.StartedAt = DateTime.UtcNow)
                .Schedule(
                    this.RenewalTimeout,
                    context => new InstagramRenewalTimeoutEvent(context.Saga.CorrelationId)
                )
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
                .Unschedule(this.RenewalTimeout)
                .TransitionTo(this.InstagramAccountDataFetched)
        );

        this.CompositeEvent(
            () => this.InstagramAccessRenewalCompleted,
            state => state.RenewalCompletedStatus,
            this.EventAccountDataFetched
        );

        this.DuringAny(
            this.When(this.InstagramAccessRenewalCompleted)
                .Unschedule(this.RenewalTimeout)
                .Publish(context =>
                    new InstagramAccessRenewalCompletedIntegrationEvent(
                        Guid.NewGuid(),
                        DateTime.UtcNow,
                        context.Saga.CorrelationId
                    )
                )
                .Finalize(),
            this.When(this.EventRenewalFailed)
                .Unschedule(this.RenewalTimeout)
                .Publish(context =>
                    new InstagramRenewalAccessFailureCleanedUpIntegrationEvent(
                        Guid.NewGuid(),
                        DateTime.UtcNow,
                        context.Saga.CorrelationId,
                        "Instagram access renewal failed"
                    )
                )
                .Finalize(),
            this.When(this.RenewalTimeout!.Received)
                .Publish(context =>
                    new InstagramRenewalAccessFailureCleanedUpIntegrationEvent(
                        Guid.NewGuid(),
                        DateTime.UtcNow,
                        context.Saga.CorrelationId,
                        "Instagram access renewal timed out - user can try again"
                    )
                )
                .Finalize()
        );
    }
}
