using GomokuWebAPI.Model.Enums;

namespace GomokuWebAPI.Model
{
    public class AuditableEntity
    {
        public long Id { get; set; }
        public Status Status { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset? LastModifiedDate { get; set; }
        public DateTimeOffset? InactivatedDate { get; set; }
    }
}
