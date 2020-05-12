using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Automatonymous;

namespace MassTransitDuplicateSagaHandlersIssue
{
    [Table("TestSaga", Schema = "dbo")]
    public class TestSaga : SagaStateMachineInstance
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid CorrelationId { get; set; }
        [MaxLength(64)]
        public string CurrentState { get; set; }

        public string CollectionId { get; set; }

        public string Name { get; set; }
    }
}