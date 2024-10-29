using CrossPlanner.Repository.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace CrossPlanner.Repository.Wrapper
{
    public interface IRepositoryWrapper
    {
        IMailServerRepository MailServerRepository { get; }

        IAffiliateRepository AffiliateRepository { get; }

        IAffiliateUsersRepository AffiliateUsersRepository { get; }

        IApplicationUserRepository ApplicationUserRepository { get; }

        IMembershipPlanRepository MembershipPlanRepository { get; }

        IMembershipRepository MembershipRepository { get; }

        IUserStripeAffiliateRepository UserStripeAffiliateRepository { get; }

        IRefundRepository RefundRepository { get; }

        IClassTypeRepository ClassTypeRepository { get; }

        IScheduledClassRepository ScheduledClassRepository { get; }

        IScheduledClassReservationRepository ScheduledClassReservationRepository { get; }

        IWorkoutRepository WorkoutRepository { get; }

        INotificationRepository NotificationRepository { get; }

        IMailGroupRepository MailGroupRepository { get; }

        IMailDetailsRepository MailDetailsRepository {  get; }

        IMailGroupDetailsRepository MailGroupDetailsRepository { get; }

        void Save();

        Task SaveAsync();

        IDbContextTransaction BeginTransaction();
    }
}