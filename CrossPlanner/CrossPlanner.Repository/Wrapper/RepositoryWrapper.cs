using CrossPlanner.Domain.Context;

namespace CrossPlanner.Repository.Wrapper
{
    public class RepositoryWrapper : IRepositoryWrapper
    {
        private readonly ApplicationContext _applicationContext;

        public RepositoryWrapper(ApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        public void Save()
        {
            _applicationContext.SaveChanges();
        }
    }
}