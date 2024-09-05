﻿using CrossPlanner.Domain.Context;
using CrossPlanner.Domain.Models;
using CrossPlanner.Repository.Interfaces;

namespace CrossPlanner.Repository.Repository
{
    public class ScheduledClassWorkoutRepository : RepositoryBase<ScheduledClassWorkout>, IScheduledClassWorkoutRepository
    {
        public ScheduledClassWorkoutRepository(ApplicationContext applicationContext) : base(applicationContext)
        {
        }
    }
}