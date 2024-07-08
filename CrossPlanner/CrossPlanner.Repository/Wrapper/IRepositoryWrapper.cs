using CrossPlanner.Repository.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace CrossPlanner.Repository.Wrapper
{
    public interface IRepositoryWrapper
    {
        IMailServerRepository MailServerRepository { get; }

        IAffiliateRepository AffiliateRepository { get; }

        IAffiliateUsersRepository AffiliateUsersRepository { get; }

        void Save();

        Task SaveAsync();

        IDbContextTransaction BeginTransaction();
    }
}