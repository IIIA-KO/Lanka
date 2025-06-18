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
    public Event InstagramLinkingCompleted { get; init; }

    public LinkInstagramSaga()
    {
        this.Event(() => this.EventAccountLinked, c => c.CorrelateById(m => m.Message.UserId));
        this.Event(() => this.EventAccountDataFetched, c => c.CorrelateById(m => m.Message.UserId));

        this.InstanceState(s => s.CurrentState);

        this.Initially(
            this.When(this.EventAccountLinked)
                .Publish(context =>
                    new InstagramAccountLinkingStartedIntegrationEvent(
                        context.Message.Id,
                        context.Message.OccurredOnUtc,
                        context.Message.UserId,
                        context.Message.Code
                    )
                )
                .TransitionTo(this.LinkingStarted)
        );

        this.During(this.LinkingStarted,
            this.When(this.EventAccountDataFetched)
                .TransitionTo(this.InstagramAccountDataFetched)
        );

        this.CompositeEvent(
            () => this.InstagramLinkingCompleted,
            state => state.LinkingCompletedStatus,
            this.EventAccountDataFetched
        );

        this.DuringAny(
            this.When(this.InstagramLinkingCompleted)
                .Publish(context =>
                    new InstagramAccountLinkingCompletedIntegrationEvent(
                        Guid.NewGuid(),
                        DateTime.UtcNow,
                        context.Saga.CorrelationId
                    )
                )
                .Finalize()
        );
    }
}
