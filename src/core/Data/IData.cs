﻿using System;
using System.Collections.Generic;
using System.Linq;
using core.Models;

namespace core.Data
{
	public interface IDb<T> where T:IEntity
	{
		string Connection { get;}
		IRepository<T> Repository { get; set; }
	}

    public interface IRepository<T> where T:IEntity
	{
        IQueryable<T> List { get;}
        T Find(string Id);
        void Add(T entity);
        void Update(T entity);
        void Delete(T entity);		
	}

}