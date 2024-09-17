using CrossPlanner.Domain.Context;
using CrossPlanner.Repository.Interfaces;
using CrossPlanner.Repository.Repository;
using Microsoft.EntityFrameworkCore.Storage;

namespace CrossPlanner.Repository.Wrapper
{
    public class RepositoryWrapper : IRepositoryWrapper
    {
        private readonly ApplicationContext _applicationContext;
        private IDbContextTransaction _transaction;

        public RepositoryWrapper(ApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        private IMailServerRepository _mailServerRepository;

        public IMailServerRepository MailServerRepository
        {
            get
            {
                if (_mailServerRepository == null)
                {
                    _mailServerRepository = new MailServerRepository(_applicationContext);
                }

                return _mailServerRepository;
            }
        }

        private IAffiliateRepository _affiliateRepository;

        public IAffiliateRepository AffiliateRepository
        {
            get
            {
                if (_affiliateRepository == null)
                {
                    _affiliateRepository = new AffiliateRepository(_applicationContext);
                }

                return _affiliateRepository;
            }
        }

        private IAffiliateUsersRepository _affiliateUsersRepository;

        public IAffiliateUsersRepository AffiliateUsersRepository
        {
            get
            {
                if (_affiliateUsersRepository == null)
                {
                    _affiliateUsersRepository = new AffiliateUsersRepository(_applicationContext);
                }

                return _affiliateUsersRepository;
            }
        }

        private IApplicationUserRepository _applicationUserRepository;

        public IApplicationUserRepository ApplicationUserRepository
        {
            get
            {
                if (_applicationUserRepository == null)
                {
                    _applicationUserRepository = new ApplicationUserRepository(_applicationContext);
                }

                return _applicationUserRepository;
            }
        }
        
        private IMembershipPlanRepository _membershipPlanRepository;

        public IMembershipPlanRepository MembershipPlanRepository
        {
            get
            {
                if (_membershipPlanRepository == null)
                {
                    _membershipPlanRepository = new MembershipPlanRepository(_applicationContext);
                }

                return _membershipPlanRepository;
            }
        }

        private IMembershipRepository _membershipRepository;

        public IMembershipRepository MembershipRepository
        {
            get
            {
                if (_membershipRepository == null)
                {
                    _membershipRepository = new MembershipRepository(_applicationContext);
                }

                return _membershipRepository;
            }
        }

        private IUserStripeAffiliateRepository _userStripeAffiliateRepository;

        public IUserStripeAffiliateRepository UserStripeAffiliateRepository
        {
            get
            {
                if (_userStripeAffiliateRepository == null)
                {
                    _userStripeAffiliateRepository = new UserStripeAffiliateRepository(_applicationContext);
                }

                return _userStripeAffiliateRepository;
            }
        }

        private IRefundRepository _refundRepository;

        public IRefundRepository RefundRepository
        {
            get
            {
                if (_refundRepository == null)
                {
                    _refundRepository = new RefundRepository(_applicationContext);
                }

                return _refundRepository;
            }
        }

        private IClassTypeRepository _classTypeRepository;

        public IClassTypeRepository ClassTypeRepository
        {
            get
            {
                if (_classTypeRepository == null)
                {
                    _classTypeRepository = new ClassTypeRepository(_applicationContext);
                }

                return _classTypeRepository;
            }
        }

        private IScheduledClassRepository _scheduledClassRepository;

        public IScheduledClassRepository ScheduledClassRepository
        {
            get
            {
                if (_scheduledClassRepository == null)
                {
                    _scheduledClassRepository = new ScheduledClassRepository(_applicationContext);
                }

                return _scheduledClassRepository;
            }
        }

        private IScheduledClassReservationRepository _scheduledClassReservationRepository;

        public IScheduledClassReservationRepository ScheduledClassReservationRepository
        {
            get
            {
                if (_scheduledClassReservationRepository == null)
                {
                    _scheduledClassReservationRepository = new ScheduledClassReservationRepository(_applicationContext);
                }

                return _scheduledClassReservationRepository;
            }
        }

        private IWorkoutRepository _workoutRepository;

        public IWorkoutRepository WorkoutRepository
        {
            get
            {
                if (_workoutRepository == null)
                {
                    _workoutRepository = new WorkoutRepository(_applicationContext);
                }

                return _workoutRepository;
            }
        }

        private INotificationRepository _notificationRepository;

        public INotificationRepository NotificationRepository
        {
            get
            {
                if (_notificationRepository == null)
                {
                    _notificationRepository = new NotificationRepository(_applicationContext);
                }

                return _notificationRepository;
            }
        }

        public void Save()
        {
            _applicationContext.SaveChanges();
        }

        public async Task SaveAsync()
        {
            await _applicationContext.SaveChangesAsync();
        }

        public IDbContextTransaction BeginTransaction()
        {
            _transaction = _applicationContext.Database.BeginTransaction();
            return _transaction;
        }
    }
}