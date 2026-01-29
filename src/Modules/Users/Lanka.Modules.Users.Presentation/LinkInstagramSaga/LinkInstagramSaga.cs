using Lanka.Modules.Analytics.IntegrationEvents;
using Lanka.Modules.Users.IntegrationEvents.LinkInstagram;
using MassTransit;

namespace Lanka.Modules.Users.Presentation.LinkInstagramSaga;

public sealed class LinkInstagramSaga : MassTransitStateMachine<LinkInstagramState>
{
    public State LinkingStarted { get; init; }
    public State InstagramAccountDataFetched { get; init; }

    public Event<InstagramAccountLinkedIntegrationEvent> EventAccountLinked { get; init; }
    public Event<InstagramAccountDataFetchedIntegrationEvent> EventAccountDataFetched { get; init; }
    public Event<InstagramLinkingFailedIntegrationEvent> EventLinkingFailed { get; init; }
    public Event InstagramLinkingCompleted { get; init; }

    public Schedule<LinkInstagramState, InstagramLinkingTimeoutEvent> LinkingTimeout { get; init; }

    public LinkInstagramSaga()
    {
        this.Event(() => this.EventAccountLinked, c => c.CorrelateById(m => m.Message.UserId));
        this.Event(() => this.EventAccountDataFetched, c => c.CorrelateById(m => m.Message.UserId));
        this.Event(() => this.EventLinkingFailed, c => c.CorrelateById(m => m.Message.UserId));

        this.Schedule(
            () => this.LinkingTimeout,
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
            this.When(this.EventAccountLinked)
                .Then(context => context.Saga.StartedAt = DateTime.UtcNow)
                .Schedule(
                    this.LinkingTimeout,
                    context => new InstagramLinkingTimeoutEvent(context.Saga.CorrelationId)
                )
                .Publish(context =>
                    new InstagramAccountLinkingStartedIntegrationEvent(
                        context.Message.Id,
                        context.Message.OccurredOnUtc,
                        context.Message.UserId,
                        context.Message.Email,
                        context.Message.Code
                    )
                )
                .TransitionTo(this.LinkingStarted)
        );

        this.During(this.LinkingStarted,
            this.When(this.EventAccountDataFetched)
                .Unschedule(this.LinkingTimeout)
                .TransitionTo(this.InstagramAccountDataFetched)
        );

        this.CompositeEvent(
            () => this.InstagramLinkingCompleted,
            state => state.LinkingCompletedStatus,
            this.EventAccountDataFetched
        );

        this.DuringAny(
            this.When(this.InstagramLinkingCompleted)
                .Unschedule(this.LinkingTimeout)
                .Publish(context =>
                    new InstagramAccountLinkingCompletedIntegrationEvent(
                        Guid.NewGuid(),
                        DateTime.UtcNow,
                        context.Saga.CorrelationId
                    )
                )
                .Finalize(),
            this.When(this.EventLinkingFailed)
                .Unschedule(this.LinkingTimeout)
                .Publish(context =>
                    new InstagramAccountLinkingFailureCleanedUpIntegrationEvent(
                        Guid.NewGuid(),
                        DateTime.UtcNow,
                        context.Saga.CorrelationId,
                        "Instagram linking failed"
                    )
                )
                .Finalize(),
            this.When(this.LinkingTimeout!.Received)
                .Publish(context =>
                    new InstagramAccountLinkingFailureCleanedUpIntegrationEvent(
                        Guid.NewGuid(),
                        DateTime.UtcNow,
                        context.Saga.CorrelationId,
                        "Instagram linking timed out - user can try again"
                    )
                )
                .Finalize()
        );
    }
}
