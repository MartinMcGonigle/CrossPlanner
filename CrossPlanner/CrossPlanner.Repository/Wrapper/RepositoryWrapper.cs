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