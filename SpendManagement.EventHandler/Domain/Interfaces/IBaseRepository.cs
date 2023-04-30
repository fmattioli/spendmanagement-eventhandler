﻿namespace Domain.Interfaces
{
    public interface IBaseRepository<T> where T : class
    {
        Task AddOne(T entity);
    }
}
