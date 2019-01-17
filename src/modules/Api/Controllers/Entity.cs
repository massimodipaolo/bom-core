﻿using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using core.Extensions.Data;

namespace core.Extensions.Api.Controllers
{
    public class EntityController<T, TKey> : ControllerBase where T : class, IEntity<TKey> where TKey : IEquatable<TKey>
    {
        protected IRepository<T, TKey> _repository;

        public EntityController(IRepository<T, TKey> repository)
        {
            _repository = repository;
        }
    }

    [Route("api/[controller]")]
    public class EntityControllerWithMethods<T, TKey> : EntityController<T, TKey> where T : class, IEntity<TKey> where TKey : IEquatable<TKey>
    {
        public EntityControllerWithMethods(IRepository<T, TKey> repository): base(repository) {}

        /// <summary>
        /// Retrieves a IQueryable<typeparamref name="T"/>
        /// </summary>
        /// <remarks>Awesomeness!</remarks>
        /// <response code="200">Product created</response>
        /// <response code="400">Product has missing/invalid values</response>
        /// <response code="500">Oops! Can't create your product right now</response>
        [HttpGet]        
        public virtual IActionResult Get()
        {
            return Ok(_repository.List);
        }

        [HttpGet("{id}")]            
        public virtual IActionResult GetById(TKey id)
        {
            return Ok(_repository.Find(id));
        }

        [HttpPost]
        public virtual void Post([FromBody]T entity)
        {
            _repository.Add(entity);
        }

        [HttpPut("{id}")]
        public virtual void Put(TKey id, [FromBody]T entity)
        {

            _repository.Update(entity);
        }

        [HttpDelete("{id}")]
        public virtual void Delete([FromBody]T entity)
        {
            _repository.Delete(entity);
        }
    }
}
