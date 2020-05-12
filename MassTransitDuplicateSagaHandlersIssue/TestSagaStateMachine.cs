using System;
using Automatonymous;
using Microsoft.Extensions.Logging;

namespace MassTransitDuplicateSagaHandlersIssue
{
    public class TestSagaStateMachine : MassTransitStateMachine<TestSaga>
    {
        private readonly ILogger _log;

        public TestSagaStateMachine(ILogger<TestSagaStateMachine> log)
        {
            _log = log;
            InstanceState(x => x.CurrentState);

            Event(() => CreateSagaEvent, x => x.CorrelateBy((saga, context) => saga.Name == context.Message.Name)
                .SelectId(context => Guid.NewGuid()));
            Event(() => UpdateSagaStateEvent, x => x.CorrelateBy((saga, context) => saga.CollectionId == context.Message.CollectionId));
           
            Initially(

                When(CreateSagaEvent)
                    .Then(context =>
                    {
                        context.Instance.Name = context.Data.Name;
                        context.Instance.CollectionId = context.Data.CollectionId;
                    })
                    .Then(c => _log.LogInformation($"Creating new saga {c.Data.Name}, {c.Data.CollectionId}"))
                    .TransitionTo(Created));


            During(Created,
                When(UpdateSagaStateEvent)
                    .TransitionTo(Active));
          
            this.OnUnhandledEvent(c =>
            {
                _log.LogError($"{c.Event.Name} received in saga for collection {c.Instance?.CollectionId} while it was in the state '{c.CurrentState}'");
                return c.Throw();
            });

            SetCompletedWhenFinalized();
        }

        public Event<CreateSaga> CreateSagaEvent { get; set; }
        public Event<UpdateSagaState> UpdateSagaStateEvent { get; set; }

        public State Created { get; set; }
        public State Active { get; set; }
    }
}